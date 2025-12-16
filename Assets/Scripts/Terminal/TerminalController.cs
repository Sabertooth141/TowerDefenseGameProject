using System;
using System.Collections.Generic;
using Entity.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
        [SerializeField] private GameObject terminalPanel;
        [SerializeField] private TMP_InputField cmdInput;
        [SerializeField] private TextMeshProUGUI outputText;

        private PlayerController _playerController;
        private bool _terminalOpen;
        private List<String> _outputLines = new List<String>();
        private Dictionary<String, TerminalCommand> _commands = new Dictionary<string, TerminalCommand>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (terminalPanel == null)
            {
                Debug.LogError("TerminalController: terminalPanel is null");
            }

            if (_playerController == null)
            {
                _playerController = FindAnyObjectByType<PlayerController>();
            }

            cmdInput = terminalPanel.GetComponentInChildren<TMP_InputField>();
            if (cmdInput == null)
            {
                Debug.LogError("TerminalController: input field is null");
            }

            outputText = terminalPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (outputText == null)
            {
                Debug.LogError("TerminalController: output text field is null");
            }

            terminalPanel.SetActive(false);
            InitCommands();

            cmdInput.onSubmit.AddListener(OnCommandSubmit);
            
            AddOutput("TERMINAL V2.4.1 INITIALIZED");
            AddOutput("TYPE 'HELP' FOR AVAILABLE COMMANDS");
            AddOutput("---------------------------------------------");
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
            terminalPanel.SetActive(true);

            cmdInput.text = "";
            cmdInput.ActivateInputField();
        }

        public void CloseTerminal()
        {
            if (!_terminalOpen)
            {
                return;
            }

            _terminalOpen = false;
            terminalPanel.SetActive(false);
            _playerController.EnableMovement();
        }

        private void AddOutput(string output)
        {
            _outputLines.Add(output);

            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();
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
            if (!_commands.ContainsKey(cmd.ToUpper()))
            {
                AddOutput($"ERROR: Unknown command '{cmd}', type 'HELP' for available commands");
                cmdInput.text = "";
                cmdInput.ActivateInputField();
                return;
            }

            try
            {
                _commands[cmd.ToUpper()].callback?.Invoke(cmd.Split(' '));
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
        }

        private void ClearOutput()
        {
            _outputLines.Clear();
            outputText.text = "";
        }

        private void OnInputChanged(string command)
        {
        }
    }
}