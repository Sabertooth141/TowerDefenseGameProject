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
    public class EndSceneUIController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private TMP_InputField cmdInput;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private UISFXController sfxController;

        [Header("Terminal settings")]
        public int maxOutputLInes = 50;

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

            StartCoroutine(HandleDroneDestroyed());
        }

        private void OnDestroy()
        {
            EventHub.OnExecuteCommand -= HandleOnExecuteCmd;
            EventHub.OnCommandComplete -= HandleOnCmdComplete;
        }

        private string Ok(string t) => $"<color=#00FF00>{t}</color>";
        private string Warn(string t) => $"<color=#FFAA00>{t}</color>";
        private string Err(string t) => $"<color=#FF5555>{t}</color>";

        private IEnumerator HandleDroneDestroyed()
        {
            cmdInput.interactable = false;

            AddOutput(Err("!!! CONNECTION LOST !!!"));
            yield return Wait(0.5f, 0.9f);

            AddOutput("Attempting to re-establish uplink...");
            yield return Wait(0.6f, 1.2f);

            AddOutput($"Uplink status ........ {Err("FAILED")}");
            yield return Wait(0.3f, 0.6f);

            AddOutput($"Telemetry stream ..... {Err("OFFLINE")}");
            yield return Wait(0.3f, 0.6f);

            AddOutput("Last known coordinates: [DATA CORRUPTED]");
            yield return Wait(0.4f, 0.7f);

            AddOutput($"Hull integrity ....... {Err("CRITICAL")}");
            yield return Wait(0.3f, 0.6f);

            AddOutput($"Power levels ......... {Err("ZERO")}");
            yield return Wait(0.5f, 0.9f);

            AddOutput("");
            AddOutput($"{Warn("WARNING:")} Drone unit unresponsive.");
            yield return Wait(0.4f, 0.8f);

            AddOutput($"Autonomous recovery .. {Err("UNAVAILABLE")}");
            yield return Wait(0.4f, 0.7f);

            AddOutput($"Mission status ....... {Err("FAILED")}");
            yield return Wait(0.6f, 1.0f);

            AddOutput("");
            AddOutput("=== OPERATOR OPTIONS ===");
            yield return Wait(0.2f, 0.4f);

            AddOutput("RETRY  - Deploy replacement drone");
            AddOutput("EXIT   - Terminate operation");
            AddOutput("");
            AddOutput("Awaiting operator command...");

            RegisterCmd("RETRY", "Retry mission", _ => StartCoroutine(RetryMission()));
            RegisterCmd("EXIT", "Exit game", _ => ExitGame());

            cmdInput.interactable = true;
            cmdInput.ActivateInputField();
        }

        private IEnumerator RetryMission()
        {
            EventHub.TriggerOnExecuteCommand();

            AddOutput("Deploying replacement drone...");
            yield return Wait(1.0f, 1.5f);

            SceneManager.LoadScene("PlayScene");
        }

        private void ExitGame()
        {
            EventHub.TriggerOnExecuteCommand();
            AddOutput("Terminating operation...");
            StartCoroutine(QuitAfterDelay(1.2f));
        }

        private IEnumerator QuitAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator Wait(float min, float max)
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

            EventHub.TriggerOnExecuteCommand();
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
            if (_outputLines.Count >= maxOutputLInes)
                _outputLines.RemoveAt(0);

            _outputLines.Add(output);
            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }

        private void RegisterCmd(string cmdName, string description, Action<string[]> callback)
        {
            if (string.IsNullOrWhiteSpace(cmdName) || callback == null)
                return;

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
