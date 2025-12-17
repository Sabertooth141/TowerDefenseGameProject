using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSystem;
using Misc;
using Terminal;
using UnityEngine;

namespace Terminal
{
    public class TerminalManager : MonoBehaviour
    {
        public static TerminalManager Instance { get; private set; }

        private List<string> _potentialDirs = new List<string>();
        private List<GameObject> _activeTerminals = new List<GameObject>();
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            RegisterTerminals();
        }

        private void Awake()
        {
            EventHub.OnFilesGenerated += InitDirInTerminals;
        }

        // register and init terminals
        private void RegisterTerminals()
        {
            GameObject[] terminalsInMap = GameObject.FindGameObjectsWithTag("Terminal");
            
            foreach (GameObject terminal in terminalsInMap)
            {
                _activeTerminals.Add(terminal);
            }
        }

        private void InitDirInTerminals()
        {
            _potentialDirs = TaskManager.Instance.GetGeneratedFiles();
            int terminalIndex = 0;
            Debug.Log(_potentialDirs.Count);
            for (int i = 0; i < _potentialDirs.Count; i++)
            {
                _activeTerminals[terminalIndex].GetComponent<TerminalController>().AddDir(_potentialDirs[i]);
                terminalIndex++;
                terminalIndex %= _activeTerminals.Count;
            }
        }

        public void UnregisterTerminal(GameObject terminal)
        {
            _activeTerminals.Remove(terminal);
        }

        public List<GameObject> GetActiveTerminals()
        {
            return _activeTerminals;
        }

        public int GetTerminalCount()
        {
            return _activeTerminals.Count;
        }
    }
}