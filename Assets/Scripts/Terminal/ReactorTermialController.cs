using System.Collections;
using Entity.Player;
using EventSystem;
using Misc;
using Terminal;
using UnityEngine;

public class ReactorTermialController : TerminalController
{
    [SerializeField] private float generatorStartupTime = 10;
    [SerializeField] private float generatorCloseTime = 20;
    [SerializeField] private int progressBarLen = 40;

    private float _progress;

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
    }

    protected override void Start()
    {
        base.Start();

        EventHub.OnTryTurnOffGenerator += HandleTurnOffReactor;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventHub.OnTryTurnOffGenerator -= HandleTurnOffReactor;
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
        AddOutput("---------------------------------------------");
        AddOutput(""); // placeholder for progress bar
        int progressBarIndex = GetCurrentLineIndex() - 1;

        while (_progress < generatorStartupTime)
        {
            float increment = Random.Range(0.1f, 1f);
            _progress += increment;
            _progress = Mathf.Min(_progress, generatorStartupTime);

            // Calculate percentage for progress bar
            float percentage = _progress / generatorStartupTime;
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

    private void HandleTurnOffReactor()
    {
        AddOutput("==== EXTERNAL OVERRIDE ====");
        AddOutput("==== GENERATOR TURN OFF ====");
        StartCoroutine(TurnOffReactor());
    }

    private IEnumerator TurnOffReactor()
    {
        AddOutput("GENERATOR STOPPING FROM EXTERNAL OVERRIDE");
        AddOutput("REMOVE EXTERNAL SOURCE TO STOP");
        AddOutput("---------------------------------------------");
        AddOutput(""); // placeholder for progress bar
        int progressBarIndex = GetCurrentLineIndex() - 1;

        while (_progress < generatorCloseTime)
        {
            _progress += Time.deltaTime;
            _progress = Mathf.Min(_progress, generatorCloseTime);

            // Calculate percentage for progress bar
            float percentage = _progress / generatorCloseTime;
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

    protected override void HandlePowerDown()
    {
    }

    protected override void HandlePowerUp()
    {
    }
}