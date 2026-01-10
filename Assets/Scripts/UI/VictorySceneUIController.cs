using System;
using System.Collections;
using System.Collections.Generic;
using GameEvents;
using Terminal;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class VictorySceneUIController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private TMP_InputField cmdInput;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private UISFXController sfxController;

        [Header("Terminal settings")]
        public int maxOutputLines = 50;

        private readonly List<string> _outputLines = new();
        private readonly Dictionary<string, TerminalCommand> _commands = new();

        private bool _isExecutingCmd;

        private void Awake()
        {
            cmdInput.onSubmit.AddListener(OnCommandSubmit);
            cmdInput.onValueChanged.AddListener(OnCommandChanged);
        }

        private void Start()
        {
            EventHub.OnExecuteCommand += HandleOnExecuteCmd;
            EventHub.OnCommandComplete += HandleOnCmdComplete;

            StartCoroutine(HandleVictorySequence());
        }

        private void OnDestroy()
        {
            EventHub.OnExecuteCommand -= HandleOnExecuteCmd;
            EventHub.OnCommandComplete -= HandleOnCmdComplete;
        }

        private IEnumerator HandleVictorySequence()
        {
            cmdInput.interactable = false;

            AddOutput("Re-establishing uplink...");
            yield return RandomWait(0.3f, 0.6f);

            AddOutput("Telemetry stream ...... <color=#55FF55>ONLINE</color>");
            yield return RandomWait(0.3f, 0.6f);

            AddOutput("");
            AddOutput("Verifying retrieved data...");
            yield return RandomWait(0.6f, 1.2f);

            AddOutput(" File integrity ........ <color=#55FF55>VALID</color>");
            yield return RandomWait(0.2f, 0.5f);

            AddOutput(" Encryption status ..... <color=#55FF55>INTACT</color>");
            yield return RandomWait(0.2f, 0.5f);

            AddOutput(" Transfer checksum ..... <color=#55FF55>CONFIRMED</color>");
            yield return RandomWait(0.3f, 0.6f);

            AddOutput("");
            AddOutput("Mission status ........ <color=#55FF55>SUCCESS</color>");
            yield return RandomWait(0.5f, 0.9f);

            AddOutput("");
            AddOutput("=== OPERATION COMPLETE ===");
            yield return RandomWait(0.3f, 0.6f);

            AddOutput("All critical files successfully retrieved.");
            yield return RandomWait(0.3f, 0.6f);

            AddOutput("Drone unit returned to standby.");
            yield return RandomWait(0.3f, 0.6f);

            AddOutput("");
            AddOutput("Operator options:");
            AddOutput(" NEXT   - Proceed to next mission");
            AddOutput(" LOGOUT - Terminate session");
            AddOutput("");
            AddOutput("Awaiting operator command...");

            RegisterCmd("NEXT", "Proceed to next mission", _ =>
            {
                EventHub.TriggerOnExecuteCommand();
                SceneManager.LoadScene("PlayScene");
            });

            RegisterCmd("LOGOUT", "Terminate session", _ =>
            {
                EventHub.TriggerOnExecuteCommand();
                Application.Quit();
            });

            cmdInput.interactable = true;
            cmdInput.ActivateInputField();
        }

        private IEnumerator RandomWait(float min, float max)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
        }

        private void OnCommandChanged(string cmd)
        {
            sfxController.PlayTypingSFX();
        }

        private void OnCommandSubmit(string command)
        {
            sfxController.PlayTypingSFX();
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

            ProcessCmd(command);

            cmdInput.text = "";
            cmdInput.ActivateInputField();
        }

        private void ProcessCmd(string cmd)
        {
            string[] args = cmd.Split(" ");

            if (!_commands.TryGetValue(args[0].ToUpper(), out TerminalCommand terminalCommand))
            {
                EventHub.TriggerOnCommandCompleted();
                return;
            }

            try
            {
                terminalCommand.callback?.Invoke(args);
            }
            catch (Exception e)
            {
                AddOutput($"ERROR: {e.Message}");
            }

            EventHub.TriggerOnCommandCompleted();
        }

        public void AddOutput(string output)
        {
            sfxController.PlayTypingSFX();
            if (_outputLines.Count >= maxOutputLines)
                _outputLines.RemoveAt(0);

            _outputLines.Add(output);
            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }

        private void RegisterCmd(string cmdName, string description, Action<string[]> callback)
        {
            _commands[cmdName.ToUpper()] = new TerminalCommand
            {
                name = cmdName,
                description = description,
                callback = callback
            };
        }

        private void HandleOnExecuteCmd()
        {
            _isExecutingCmd = true;
        }

        private void HandleOnCmdComplete()
        {
            _isExecutingCmd = false;
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
