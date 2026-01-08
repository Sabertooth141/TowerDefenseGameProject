using System;
using System.Collections;
using System.Collections.Generic;
using GameEvents;
using UnityEngine.EventSystems;
using Terminal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace UI
{
    public class StartupTerminalController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private GameObject startupUIPanel;
        [SerializeField] private TMP_InputField cmdInput;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private UIController uiController;
        [SerializeField] private Image startupBackground;

        [Header("Terminal settings")]
        public int maxOutputLInes = 50;

        private readonly List<string> _outputLines = new();
        private readonly Dictionary<string, TerminalCommand> _commands = new();

        private bool _isExecutingCmd;

        private void Awake()
        {
            cmdInput.onSubmit.AddListener(OnCommandSubmit);
            startupUIPanel.SetActive(true);
        }

        private void Start()
        {
            EventHub.OnExecuteCommand += HandleOnExecuteCmd;
            EventHub.OnCommandComplete += HandleOnCmdComplete;

            StartCoroutine(HandleMissionConfirmation());
        }

        private void OnDestroy()
        {
            EventHub.OnExecuteCommand -= HandleOnExecuteCmd;
            EventHub.OnCommandComplete -= HandleOnCmdComplete;
        }

        private string Ok(string text) => $"<color=#00FF00>{text}</color>";
        private string Good(string text) => $"<color=#00FFAA>{text}</color>";

        private IEnumerator HandleMissionConfirmation()
        {
            cmdInput.interactable = false;

            AddOutput("=== OPERATION ICARUS BRIEF ===");
            yield return RandomWait(0.2f, 0.4f);
            EventHub.TriggerOnStartScene();

            AddOutput($"Target Facility: RS_{UnityEngine.Random.Range(100, 999)}");
            yield return RandomWait(0.1f, 0.25f);
            AddOutput("Objective: Data Retrieval");
            yield return RandomWait(0.2f, 0.4f);
            AddOutput("");

            AddOutput("Primary Tasks:");
            yield return RandomWait(0.1f, 0.2f);
            AddOutput(" - Activate auxiliary generator");
            yield return RandomWait(0.1f, 0.2f);
            AddOutput(" - Retrieve critical files");
            yield return RandomWait(0.3f, 0.5f);
            AddOutput("");

            AddOutput("Threat Level: UNKNOWN");
            yield return RandomWait(0.1f, 0.25f);
            AddOutput("Environmental Status: DEGRADED");
            yield return RandomWait(0.1f, 0.25f);
            AddOutput("Signal Reliability: INTERMITTENT");
            yield return RandomWait(0.4f, 0.6f);
            AddOutput("");

            AddOutput("Mission requires manual operator authorization.");
            yield return RandomWait(0.3f, 0.5f);
            AddOutput("");
            AddOutput("Proceed with mission? (Y/N)");

            RegisterCmd("Y", "Confirm mission start", _ => StartCoroutine(HandleMissionAccepted()));
            RegisterCmd("N", "Abort mission", _ => StartCoroutine(HandleMissionAborted()));

            cmdInput.interactable = true;
            cmdInput.ActivateInputField();
        }

        private IEnumerator HandleMissionAccepted()
        {
            EventHub.TriggerOnExecuteCommand();

            UnregisterCmd("Y");
            UnregisterCmd("N");

            AddOutput("Operator confirmation received.");
            yield return RandomWait(0.3f, 0.6f);
            AddOutput($"Mission status: {Ok("AUTHORIZED")}");
            yield return RandomWait(0.5f, 0.8f);
            AddOutput("");

            EventHub.TriggerOnCommandCompleted();
            StartCoroutine(HandleBootSequence());
        }

        private IEnumerator HandleMissionAborted()
        {
            EventHub.TriggerOnExecuteCommand();

            UnregisterCmd("Y");
            UnregisterCmd("N");

            AddOutput("Operator declined mission authorization.");
            yield return RandomWait(0.3f, 0.6f);
            AddOutput("<color=#FF5555>Mission status: ABORTED</color>");
            yield return RandomWait(0.5f, 0.8f);

            EventHub.TriggerOnCommandCompleted();
            Application.Quit();
        }

        private IEnumerator HandleBootSequence()
        {
            cmdInput.interactable = false;

            AddOutput("DRONE-OS v0.9.14 [Build 2026.01]");
            yield return RandomWait(0.1f, 0.2f);

            AddOutput("Copyright (C) Autonomous Systems Division");
            yield return RandomWait(0.2f, 0.4f);
            AddOutput("");

            AddOutput("Initializing hardware...");
            yield return RandomWait(0.4f, 0.8f);

            yield return RunCheck("CPU");
            yield return RunCheck("IMU");
            yield return RunCheck("Gyroscope");
            yield return RunCheck("Accelerometer");
            yield return RunCheck("Magnetometer");

            AddOutput("");

            AddOutput("Loading firmware modules...");
            yield return RandomWait(0.3f, 0.7f);

            yield return RunCheck("nav.core");
            yield return RunCheck("cam.driver");
            yield return RunCheck("comms.link");
            yield return RunCheck("power.mgmt");

            StartCoroutine(FadeBackground(6f, 0f));
            AddOutput("");

            AddOutput("Running system checks...");
            yield return RandomWait(0.3f, 0.7f);

            int battery = UnityEngine.Random.Range(50, 100);
            yield return RunCheck("Battery level", Ok($"{battery}%"));
            yield return RunCheck("Signal integrity", Good("GOOD"));
            yield return RunCheck("GPS lock", Good("ACQUIRED"), 0.6f, 1.2f);

            AddOutput("");

            AddOutput("Mounting subsystems...");
            yield return RandomWait(0.3f, 0.6f);

            yield return RunCheck("/dev/camera0", Ok("READY"));
            yield return RunCheck("/dev/motors", Ok("READY"));
            yield return RunCheck("/dev/sensors", Ok("READY"));

            AddOutput("");

            AddOutput($"System status: {Ok("STANDBY")}");
            yield return RandomWait(0.1f, 0.6f);
            AddOutput("Initiating Operator Control");
            yield return RandomWait(0.5f, 0.9f);

            if (startupBackground != null)
            {
                Color c = startupBackground.color;
                startupBackground.color = new Color(c.r, c.g, c.b, 0f);
            }

            startupUIPanel.SetActive(false);
            EventHub.TriggerOnPlayerControl();
        }

        private IEnumerator FadeBackground(float duration, float targetAlpha)
        {
            if (startupBackground == null)
                yield break;

            Color color = startupBackground.color;
            float startAlpha = color.a;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                startupBackground.color = new Color(color.r, color.g, color.b, a);
                yield return null;
            }

            startupBackground.color = new Color(color.r, color.g, color.b, targetAlpha);
        }

        private IEnumerator RunCheck(
            string label,
            string result = null,
            float checkMin = 0.3f,
            float checkMax = 0.8f,
            float resultMin = 0.05f,
            float resultMax = 0.15f
        )
        {
            AddOutput($" {label} ...............");
            yield return RandomWait(checkMin, checkMax);

            string finalResult = result ?? Ok("OK");
            _outputLines[^1] = $" {label} ............... {finalResult}";
            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();
            yield return RandomWait(resultMin, resultMax);
        }

        private IEnumerator RandomWait(float min, float max)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
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

        private void UnregisterCmd(string cmdName)
        {
            _commands.Remove(cmdName.ToUpper());
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
