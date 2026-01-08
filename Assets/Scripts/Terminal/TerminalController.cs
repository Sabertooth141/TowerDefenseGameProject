using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using GameEvents;
using Misc;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Terminal
{
    [Serializable]
    public class TerminalCommand
    {
        public string name;
        public string description;
        public Action<string[]> callback;
    }

    public class TerminalController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] protected GameObject terminalScreen;
        [SerializeField] protected TMP_InputField cmdInput;
        [SerializeField] protected TextMeshProUGUI outputText;
        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected Camera terminalCamera;
        [SerializeField] protected TaskManager taskManager;
        [SerializeField] protected FileUploadController uploadController;
        [SerializeField] protected UIController uiController;

        [Header("Terminal settings")]
        public int maxOutputLInes = 50;
        protected PlayerController playerController;

        private bool _terminalOpen;
        private bool _terminalFocused;
        private bool _isExecutingCmd;
        private List<String> _outputLines = new();
        private Dictionary<String, TerminalCommand> _commands = new();
        private Dictionary<string, int> _storedDir = new();
        private Coroutine _runningCoroutine;

        private readonly WaitForSeconds _wait10ms = new(0.01f);

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected virtual void Start()
        {
            InitCommands();

            EventHub.OnExecuteCommand += HandleOnExecuteCmd;
            EventHub.OnCommandComplete += HandleOnCmdComplete;
            EventHub.OnGeneratorStart += HandlePowerUp;
            EventHub.OnGeneratorTurnOff += HandlePowerDown;

            cmdInput.onSubmit.AddListener(OnCommandSubmit);

            AddOutput("TERMINAL V2.4.1 INITIALIZED");
            AddOutput("TYPE 'HELP' FOR AVAILABLE COMMANDS");
            AddOutput("---------------------------------------------");

            cmdInput.text = "";
        }

        protected virtual void OnDestroy()
        {
            EventHub.OnExecuteCommand -= HandleOnExecuteCmd;
            EventHub.OnCommandComplete -= HandleOnCmdComplete;
            EventHub.OnGeneratorStart -= HandlePowerUp;
            EventHub.OnGeneratorTurnOff -= HandlePowerDown;
        }

        protected virtual void HandlePowerUp()
        {
            terminalScreen.SetActive(true);
        }

        protected virtual void HandlePowerDown()
        {
            if (_runningCoroutine != null)
            {
                StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }
            CloseTerminal();
            terminalScreen.SetActive(false);
        }

        private void HandleOnCmdComplete()
        {
            _isExecutingCmd = false;
        }

        private void HandleOnExecuteCmd()
        {
            _isExecutingCmd = true;
        }

        protected virtual void Awake()
        {
            if (playerController == null)
            {
                playerController = FindAnyObjectByType<PlayerController>();
            }

            if (terminalScreen == null)
            {
                Debug.LogError("TerminalController: terminal panel is null");
            }

            if (cmdInput == null)
            {
                Debug.LogError("TerminalController: input field is null");
            }

            if (outputText == null)
            {
                Debug.LogError("TerminalController: output text field is null");
            }

            if (terminalCamera == null)
            {
                Debug.LogError("TerminalController: camera field is null");
            }

            if (taskManager == null)
            {
                Debug.LogError("TerminalController: task manager is null");
            }

            if (uploadController == null)
            {
                Debug.LogError("TerminalController: upload controller is null");
            }

            if (scrollRect == null)
            {
                Debug.LogError("TerminalController: scrollRect is null");
            }

            terminalScreen.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_terminalOpen)
            {
                return;
            }

            if (_terminalFocused)
            {
                if (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    UnfocusTerminal();
                }
            }
        }

        public void StartTerminal()
        {
            if (_terminalOpen)
            {
                return;
            }

            _terminalOpen = true;
            FocusTerminal();
        }

        public void CloseTerminal()
        {
            if (!_terminalOpen)
            {
                return;
            }

            _terminalOpen = false;
            UnfocusTerminal();
        }

        private void FocusTerminal()
        {
            _terminalFocused = true;

            uiController.HideHUDPanel();
            terminalCamera.enabled = true;

            UnlockInput();

            int len = cmdInput.text.Length;
            cmdInput.caretPosition = len;
            cmdInput.selectionAnchorPosition = len;
            cmdInput.selectionFocusPosition = len;

            EventHub.TriggerOnTerminalStatusChanged(true, terminalScreen.transform);
        }

        public void UnfocusTerminal()
        {
            _terminalFocused = false;
            _terminalOpen = false;

            uiController.ShowHUDPanel();
            // terminalCamera.enabled = false;

            LockInput();

            // Clear selected UI so keyboard doesn’t go to TMP
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            EventHub.TriggerOnTerminalStatusChanged(false, terminalScreen.transform);
        }

        public void LockInput()
        {
            cmdInput.DeactivateInputField();
            cmdInput.interactable = false;
        }

        public void UnlockInput()
        {
            cmdInput.interactable = true;
            cmdInput.ActivateInputField();
        }
        
        public void AddOutput(string output)
        {
            if (_outputLines.Count >= 50)
            {
                _outputLines.RemoveAt(0);
            }

            _outputLines.Add(output);
            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public int GetCurrentLineIndex()
        {
            return _outputLines.Count - 1;
        }

        public void UpdateOutputLine(int lineIndex, string message)
        {
            _outputLines[lineIndex] = message;
            outputText.text = string.Join("\n", _outputLines);
        }

        private void OnCommandSubmit(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                cmdInput.ActivateInputField();
                return;
            }

            if (_isExecutingCmd)
            {
                cmdInput.text = "";
                cmdInput.ActivateInputField();
                return;
            }

            EventHub.TriggerOnExecuteCommand();
            ProcessCmd(command);

            cmdInput.text = "";
            cmdInput.ActivateInputField();
        }

        private void ProcessCmd(string cmd)
        {
            string[] args = cmd.Split(" ");
            if (!_commands.ContainsKey(args[0].ToUpper()))
            {
                AddOutput($"ERROR: Unknown command '{cmd}', type 'HELP' for available commands");
                cmdInput.text = "";
                cmdInput.ActivateInputField();
                EventHub.TriggerOnCommandCompleted();
                return;
            }

            try
            {
                _commands[args[0].ToUpper()].callback?.Invoke(args);
            }
            catch (Exception e)
            {
                AddOutput($"ERROR: Command execution failed - {e.Message}");
                EventHub.TriggerOnCommandCompleted();
            }

            cmdInput.text = "";
            cmdInput.ActivateInputField();
        }

        protected void RegisterCmd(string cmdName, string description, Action<string[]> callback)
        {
            if (String.IsNullOrWhiteSpace(cmdName) || String.IsNullOrWhiteSpace(description))
            {
                return;
            }

            if (callback == null)
            {
                return;
            }

            _commands[cmdName.ToUpper()] = new TerminalCommand()
            {
                name = cmdName,
                description = description,
                callback = callback
            };
        }

        protected virtual void InitCommands()
        {
            // HELP
            RegisterCmd("HELP",
                "Lists all available commands",
                (args) =>
                {
                    AddOutput("==== AVAILABLE COMMANDS ====");
                    foreach (var cmd in _commands)
                    {
                        AddOutput($"{cmd.Key}: {cmd.Value.description}");
                    }
                    EventHub.TriggerOnCommandCompleted();
                });

            // CLEAR
            RegisterCmd("CLR",
                "Clears terminal output",
                (args) =>
                {
                    ClearScreen();
                    EventHub.TriggerOnCommandCompleted();
                });

            // DIR
            RegisterCmd("DIR",
                "Shows all directories",
                (args) =>
                {
                    AddOutput("==== DIRECTORIES ====");
                    foreach (var dir in _storedDir)
                    {
                        AddOutput($"[{dir.Key},  {dir.Value}KB]");
                    }

                    AddOutput("---------------------------------------------");
                    EventHub.TriggerOnCommandCompleted();
                });

            // UPLOAD
            RegisterCmd("UPLOAD", "Available Syntax: 'UPLOAD <filename>'", HandleUpload);
        }

        public void ClearScreen()
        {
            _outputLines.Clear();
            outputText.text = "";
            AddOutput("TERMINAL V2.4.1 INITIALIZED");
            AddOutput("TYPE 'HELP' FOR AVAILABLE COMMANDS");
            AddOutput("---------------------------------------------");
        }

        private void HandleUpload(string[] args)
        {
            _runningCoroutine = StartCoroutine(UploadFiles(args));
        }

        private IEnumerator UploadFiles(string[] args)
        {
            AddOutput("==== FILE UPLOAD ====");
            yield return new WaitForSeconds(0.1f);
            // verify args
            if (args.Length >= 3 || args.Length == 1)
            {
                AddOutput("UNKNOWN ARGUMENT FOR 'UPLOAD'");
                AddOutput("Correct Syntax: UPLOAD <filename>");
                EventHub.TriggerOnCommandCompleted();
                yield break;
            }

            if (!_storedDir.ContainsKey(args[1].ToLower()))
            {
                AddOutput("Directory not found");
                EventHub.TriggerOnCommandCompleted();
                yield break;
            }

            uploadController.StartVerification(args);
            _runningCoroutine = null;
        }

        public void AddDir(string dir, int fileSize)
        {
            _storedDir.Add(dir, fileSize);
        }

        private void OnInputChanged(string command)
        {
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                return;

            StartCoroutine(DelayedForceFocus());
        }

        private IEnumerator DelayedForceFocus()
        {
            yield return null;
            yield return null; 
            ForceFocusInput();
        }

        private void ForceFocusInput()
        {
            if (!cmdInput || !cmdInput.interactable)
                return;


            cmdInput.DeactivateInputField();


            EventSystem.current.SetSelectedGameObject(null);


            StartCoroutine(RebindInputField());
        }

        private IEnumerator RebindInputField()
        {
            yield return null; 
            yield return null;


            EventSystem.current.SetSelectedGameObject(cmdInput.gameObject);


            cmdInput.ActivateInputField();

 
            cmdInput.Select();
            cmdInput.MoveTextEnd(false);
        }

        private void OnEnable()
        {
            StartCoroutine(DelayedForceFocus());
        }
        
        private void OnApplicationPause(bool paused)
        {
            if (!paused)
            {
                StartCoroutine(DelayedForceFocus());
            }
        }
    }
}