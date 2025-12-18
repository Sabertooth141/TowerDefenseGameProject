using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using EventSystem;
using Misc;
using TMPro;
using UI;
using UnityEngine;
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
        [SerializeField] private GameObject terminalScreen;
        [SerializeField] private TMP_InputField cmdInput;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private Camera terminalCamera;
        [SerializeField] private TaskManager taskManager;

        private PlayerController _playerController;
        private UIController _uiController;
        private bool _terminalOpen;
        private List<String> _outputLines = new();
        private Dictionary<String, TerminalCommand> _commands = new();
        private List<string> _storedDir = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitCommands();

            cmdInput.onSubmit.AddListener(OnCommandSubmit);

            AddOutput("TERMINAL V2.4.1 INITIALIZED");
            AddOutput("TYPE 'HELP' FOR AVAILABLE COMMANDS");
            AddOutput("---------------------------------------------");
        }

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = FindAnyObjectByType<PlayerController>();
            }
            
            _uiController = _playerController.gameObject.GetComponent<UIController>();

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

            if (scrollRect == null)
            {
                Debug.LogError("TerminalController: scrollRect is null");
            }
            else
            {
                contentRect = scrollRect.content;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_terminalOpen)
            {
                if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
                {
                    CloseTerminal();
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
            _uiController.HideHUDPanel();

            cmdInput.text = "";
            cmdInput.ActivateInputField();

            terminalCamera.enabled = true;

            EventHub.TriggerOnTerminalStatusChanged(true, terminalScreen.transform);
        }

        public void CloseTerminal()
        {
            if (!_terminalOpen)
            {
                return;
            }

            _uiController.ShowHUDPanel();
            cmdInput.DeactivateInputField();
            _terminalOpen = false;

            terminalCamera.enabled = false;

            EventHub.TriggerOnTerminalStatusChanged(false, terminalScreen.transform);
        }

        private void AddOutput(string output)
        {
            _outputLines.Add(output);
            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void OnCommandSubmit(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                cmdInput.ActivateInputField();
                return;
            }

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
                return;
            }

            try
            {
                _commands[args[0].ToUpper()].callback?.Invoke(args);
            }
            catch (Exception e)
            {
                AddOutput($"ERROR: Command execution failed - {e.Message}");
            }

            cmdInput.text = "";
            cmdInput.ActivateInputField();
        }

        private void RegisterCmd(string cmdName, string description, Action<string[]> callback)
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

        private void InitCommands()
        {
            // HELP
            RegisterCmd("HELP", "Lists all available commands", (args) =>
            {
                AddOutput("==== AVAILABLE COMMANDS ====");
                foreach (var cmd in _commands)
                {
                    AddOutput($"{cmd.Key}: {cmd.Value.description}");
                }
            });

            // CLEAR
            RegisterCmd("CLR", "Clears terminal output", (args) =>
            {
                ClearOutput();
                AddOutput("TERMINAL V2.4.1 INITIALIZED");
                AddOutput("TYPE 'HELP' FOR AVAILABLE COMMANDS");
                AddOutput("---------------------------------------------");
            });

            // DIR
            RegisterCmd("DIR", "Shows all directories", (args) =>
            {
                AddOutput("==== DIRECTORIES ====");
                foreach (var dir in _storedDir)
                {
                    AddOutput($"{dir}");
                }

                AddOutput("---------------------------------------------");
            });

            // UPLOAD
            RegisterCmd("UPLOAD", "Available Syntax: 'UPLOAD <filename>'", HandleUpload);
        }

        private void HandleUpload(string[] args)
        {
            StartCoroutine(Verification(args));
        }

        private IEnumerator Verification(string[] args)
        {
            AddOutput("==== FILE UPLOAD ====");
            yield return new WaitForSeconds(0.1f);
            // verify args
            if (args.Length >= 3 || args.Length == 1)
            {
                AddOutput("UNKNOWN ARGUMENT FOR 'UPLOAD'");
                AddOutput("Correct Syntax: UPLOAD <filename>");
                yield break;
            }

            AddOutput("CLIENT: Requesting upload persmission...");
            AddOutput("---------------------------------------------");
            yield return new WaitForSeconds(0.2f);
            int winID = Random.Range(20000, 65535);
            string[] reqMsg = GenerateReq(winID);
            foreach (var line in reqMsg)
            {
                AddOutput(line);
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(Random.Range(0.4f, 1.0f));
            AddOutput("---------------------------------------------");

            AddOutput("SERVER: Receiving challenge message...");
            yield return new WaitForSeconds(0.2f);
            AddOutput("---------------------------------------------");
            string[] serverChallenge = taskManager.SendChallengeMsg(winID);

            foreach (var line in serverChallenge)
            {
                AddOutput(line);
                yield return new WaitForSeconds(0.05f);
            }
            AddOutput("---------------------------------------------");
        }

        private string[] GenerateReq(int winID)
        {
            return new[]
            {
                "[CLIENT -> SERVER]:",
                "flags: SYN",
                "type: <upload>",
                $"WIN: {winID}",
                $"session-id: sess_{name + GetInstanceID()}",
                $"session-time: {Time.deltaTime}"
            };
        }

        private string[] GenerateChallenge(string filename)
        {
            return new[]
            {
                "flags: SYN-ACK", "type: <upload>", "protoc_ver: 1.1", "timestamp: {{Time.deltaTime}}",
                "filename: {filename}",
                $"client_id: {name}-{GetInstanceID()}"
            };
        }

        private IEnumerator UploadFiles(string[] arg)
        {
            yield return null;
        }

        private void ClearOutput()
        {
            _outputLines.Clear();
            outputText.text = "";
        }

        public void AddDir(string dir)
        {
            _storedDir.Add(dir);
        }

        private void OnInputChanged(string command)
        {
        }
    }
}