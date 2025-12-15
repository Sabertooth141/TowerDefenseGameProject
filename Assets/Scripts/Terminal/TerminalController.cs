using System;
using System.Collections.Generic;
using Entity.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Terminal
{
    public class TerminalController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private GameObject terminalPanel;
        [SerializeField] private TMP_InputField cmdInput;
        [SerializeField] private TextMeshProUGUI outputText;

        private PlayerController _playerController;
        private bool _terminalOpen;
        private List<String> _outputLines = new List<String>();

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
            
            cmdInput.onSubmit.AddListener(OnCommandSubmit);
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
            
            outputText.text = string.Join("\n",  _outputLines);
            
            Canvas.ForceUpdateCanvases();
        }

        private void OnCommandSubmit(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                cmdInput.ActivateInputField();
                return;
            }
            AddOutput($"> {command}");

            cmdInput.text = "";
            cmdInput.ActivateInputField();
        }

        private void OnInputChanged(string command)
        {
            
        }
    }
}