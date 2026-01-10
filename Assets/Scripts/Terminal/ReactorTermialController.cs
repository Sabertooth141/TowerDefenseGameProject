using System.Collections;
using Entity.Player;
using GameEvents;
using Misc;
using UI;
using UnityEngine;

namespace Terminal
{
    public class ReactorTerminalController : TerminalController
    {
        [SerializeField] private float generatorStartupTime = 10;
        [SerializeField] private float generatorCloseTime = 20;
        [SerializeField] private int progressBarLen = 40;
    
        private Coroutine _generatorOffCoroutine;

        protected override void Awake()
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

            if (scrollRect == null)
            {
                Debug.LogError("TerminalController: scrollRect is null");
            }

            interactable = true;
        }

        protected override void Start()
        {
            base.Start();

            EventHub.OnTryTurnOffGenerator += HandleTurnOffGenerator;
            EventHub.OnStopTurnOffGenerator += HandleStopTurnOffGenerator;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHub.OnTryTurnOffGenerator -= HandleTurnOffGenerator;
            EventHub.OnStopTurnOffGenerator -= HandleStopTurnOffGenerator;
        }

        protected override void InitCommands()
        {
            base.InitCommands();
            RegisterCmd("GEN_START", "Activate the generator", HandleReactorStartup);
        }

        private void HandleReactorStartup(string[] args)
        {
            AddOutput("==== GENERATOR START ===="); 
            StartCoroutine(StartReactor());
        }

        private IEnumerator StartReactor()
        {
            AddOutput("Initializing Generator Starting Sequence...");
            yield return new WaitForSeconds(0.2f);
            AddOutput("---------------------------------------------");
            AddOutput(""); // placeholder for progress bar
            
            EventHub.TriggerOnGeneratorStarting();
            
            int progressBarIndex = GetCurrentLineIndex() - 1;

            float progress = 0f;
            while (progress < generatorStartupTime)
            {
                float increment = Random.Range(0.1f, 1f);
                progress += increment;
                progress = Mathf.Min(progress, generatorStartupTime);

                // Calculate percentage for progress bar
                float percentage = progress / generatorStartupTime;
                int filled = Mathf.RoundToInt(progressBarLen * percentage);
                string bar = "[" + new string('█', filled) + new string('░', progressBarLen - filled) + "]";

                int percentDisplay = Mathf.RoundToInt(percentage * 100);

                UpdateOutputLine(progressBarIndex, $"{bar} {percentDisplay}%");

                yield return new WaitForSeconds(1);
            }

            EventHub.TriggerOnGeneratorStart();
            SceneManager.Instance.isGeneratorOn = true;
            AddOutput("GENERATOR STARTED");
            EventHub.TriggerOnCommandCompleted();
        }

        private void HandleTurnOffGenerator()
        {
            AddOutput("==== EXTERNAL OVERRIDE ====");
            AddOutput("==== GENERATOR TURN OFF ====");
            _generatorOffCoroutine = StartCoroutine(TurnOffGenerator());
        }

        private IEnumerator TurnOffGenerator()
        {
            AddOutput("GENERATOR STOPPING FROM EXTERNAL OVERRIDE");
            AddOutput("REMOVE EXTERNAL SOURCE TO STOP");
            AddOutput("---------------------------------------------");
            AddOutput(""); // placeholder for progress bar
            int progressBarIndex = GetCurrentLineIndex() - 1;

            float progress = 0f;
            while (progress < generatorCloseTime)
            {
                progress += Time.deltaTime;
                progress = Mathf.Min(progress, generatorCloseTime);

                // Calculate percentage for progress bar
                float percentage = progress / generatorCloseTime;
                int filled = Mathf.RoundToInt(progressBarLen * percentage);
                string bar = "[" + new string('█', filled) + new string('░', progressBarLen - filled) + "]";

                int percentDisplay = Mathf.RoundToInt(percentage * 100);

                UpdateOutputLine(progressBarIndex, $"{bar} {percentDisplay}%");

                yield return null;
            }
        
            EventHub.TriggerOnGeneratorTurnOff();
            SceneManager.Instance.isGeneratorOn = false;
            AddOutput("GENERATOR SHUTDOWN");
            EventHub.TriggerOnCommandCompleted();
        }

        private void HandleStopTurnOffGenerator()
        {
            if (_generatorOffCoroutine == null)
            {
                return;
            }
        
            EventHub.TriggerOnExecuteCommand();
            StopCoroutine(_generatorOffCoroutine);
            _generatorOffCoroutine = null;
        
            AddOutput("GENERATOR SHUTDOWN SEQUENCE HALTED");
            EventHub.TriggerOnCommandCompleted();
        }

        protected override void HandlePowerDown()
        {
        }

        protected override void HandlePowerUp()
        {
        }
    }
}