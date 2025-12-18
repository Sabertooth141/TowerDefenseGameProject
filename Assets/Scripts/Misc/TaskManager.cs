using System;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Misc
{
    public struct UploadTask
    {
        public string taskDir;
        public GameObject terminalToUpload;
    }

    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        [Header("FileUploadSettings")]
        [Tooltip("no special character")]
        public List<string> potentialSuffix = new();
        [Tooltip("no special character")]
        public List<string> potentialPrefix = new();
        [Tooltip("no special character")]
        public List<string> potentialExtension = new();
        public int numOfFilesToGenerate = 10;

        [Header("Task Settings")]
        public int maxNumOfTaskFiles = 3;

        private Dictionary<int, UploadTask> _activeTasks = new();
        private List<string> _generatedFiles = new();
        private List<string> _taskFiles = new();
        private int _numOfUploadedFiles = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitUploadTaskList();
        }

        private void Awake()
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

            if (potentialSuffix.Count <= 0)
            {
                Debug.LogWarning("Potential suffix is empty");
            }

            if (potentialPrefix.Count <= 0)
            {
                Debug.LogWarning("Potential prefix is empty");
            }

            if (potentialExtension.Count <= 0)
            {
                Debug.LogWarning("Potential extension is empty");
            }
        }

        private void InitUploadTaskList()
        {
            GenerateFileNames();
        }

        private void GenerateFileNames()
        {
            if (numOfFilesToGenerate <= 0)
            {
                return;
            }
            
            for (int i = 0; i < numOfFilesToGenerate; i++)
            {
                string selectedPrefix = potentialPrefix[Random.Range(0, potentialPrefix.Count)];
                string selectedSuffix = potentialSuffix[Random.Range(0, potentialSuffix.Count)];
                string selectedExtension = potentialExtension[Random.Range(0, potentialExtension.Count)];
                string fileName = selectedPrefix + "_" + selectedSuffix + "." + selectedExtension;
                _generatedFiles.Add(fileName);
                if (Random.Range(0, 100) > 60 && _taskFiles.Count < maxNumOfTaskFiles)
                {
                    _taskFiles.Add(fileName);                
                }
            }

            if (_taskFiles.Count <= 0)
            {
                _taskFiles.Add(_generatedFiles[Random.Range(0, _generatedFiles.Count)]);
            }
            
            EventHub.TriggerOnFilesGenerated();
        }

        public List<string> GetGeneratedFiles()
        {
            if (_generatedFiles.Count == 0)
            {
                return new List<string>();
            }
            return _generatedFiles;
        }

        public List<string> GetTaskFiles()
        {
            return _taskFiles;
        }

        public String[] SendChallengeMsg(int winID)
        {
            string allowedExtensions = "";
            foreach (var extension in potentialExtension)
            {
                allowedExtensions += extension + ", ";
            }
            return new[]
            {
                "[SERVER -> CLIENT]:",
                "flags: SYN-ACK",
                "type: <upload-challenge>",
                $"WIN: {winID}",
                $"allowed-types: {allowedExtensions}",
                $"server-id: sev_{name + GetInstanceID()}",
                $"session-time: {Time.deltaTime}"
            };
        }
    }
}