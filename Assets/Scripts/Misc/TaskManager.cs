using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEvents;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Misc
{
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        [Header("FileUploadSettings")]
        [Tooltip("no special character")] public List<string> potentialSuffix = new();
        [Tooltip("no special character")] public List<string> potentialPrefix = new();
        [Tooltip("no special character")] public List<string> potentialExtension = new();
        public int numOfFilesToGenerate = 10;
        [SerializeField] private int maxFileSize = 1048;

        [Header("Task Settings")]
        public int maxNumOfTaskFiles = 3;

        private Dictionary<string, int> _generatedFiles = new();
        private Dictionary<string, int> _taskFiles = new();
        private bool _initialized;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            EventHub.OnTerminalsRegistered += InitUploadTaskList;

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

        private void OnDestroy()
        {
            EventHub.OnTerminalsRegistered -= InitUploadTaskList;
        }

        private void InitUploadTaskList()
        {
            if (_initialized)
            {
                return;
            }
            
            _initialized = true;
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
                int selectedPrefix = Random.Range(0, potentialPrefix.Count);
                int selectedSuffix = Random.Range(0, potentialSuffix.Count);
                int selectedExtension = Random.Range(0, potentialExtension.Count);
                string fileName = potentialPrefix[selectedPrefix] + "_" +
                                  potentialSuffix[selectedSuffix] + "." +
                                  potentialExtension[selectedExtension];
                
                while (_generatedFiles.ContainsKey(fileName))
                {
                    fileName = potentialPrefix[(selectedPrefix + 2) %  potentialPrefix.Count] + "_" +
                               potentialSuffix[(selectedSuffix + 1) % potentialSuffix.Count] + "." +
                               potentialExtension[selectedExtension];
                }

                int fileSize = Random.Range(0, maxFileSize);
                _generatedFiles.Add(fileName, fileSize);
                if (Random.Range(0, 100) > 60 && _taskFiles.Count < maxNumOfTaskFiles)
                {
                    _taskFiles.Add(fileName, fileSize);
                }
            }

            if (_taskFiles.Count <= 0)
            {
                _taskFiles.Add(_generatedFiles.ElementAt(0).Key, _generatedFiles.ElementAt(0).Value);
            }

            EventHub.TriggerOnFilesGenerated();
        }

        public Dictionary<string, int> GetGeneratedFiles()
        {
            if (_generatedFiles.Count == 0)
            {
                return new Dictionary<string, int>();
            }

            return _generatedFiles;
        }

        public Dictionary<string, int> GetTaskFiles()
        {
            return _taskFiles;
        }

        public KeyValuePair<string, int> GetFileByName(string fileName)
        {
            int fileSize;
            if (_taskFiles.TryGetValue(fileName, out fileSize))
            {
                return new KeyValuePair<string, int>(fileName, fileSize);
            }

            return new KeyValuePair<string, int>(fileName, 0);
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

        public bool VerifyFilename(string filename, int winID, out string[] response)
        {
            if (_taskFiles.Count <= 0 || !_taskFiles.ContainsKey(filename))
            {
                response = new[]
                {
                    "[SERVER -> CLIENT]:",
                    "flags: RST, ACK",
                    "type: <upload-rejected>",
                    "status: 401 Unauthorized",
                    $"WIN: {winID}",
                    $"server-id: sev_{name + GetInstanceID()}",
                    $"error-code: ERR_CHALLENGE_FAILED",
                    $"error-msg: Client response verification failed",
                    $"expected-hash: [REDACTED]",
                    $"received-hash: [INVALID]",
                    $"session-time: {Time.deltaTime}",
                    "connection: TERMINATED"
                };
                return false;
            }

            response = new[]
            {
                "[SERVER -> CLIENT]:",
                "flags: ACK",
                "type: <upload-authorized>",
                "status: 200 OK",
                $"WIN: {winID}",
                $"server-id: sev_{name + GetInstanceID()}",
                $"upload-token: tkn_{Random.Range(10000, 99999)}",
                $"session-time: {Time.deltaTime}",
                "connection: ESTABLISHED"
            };
            return true;
        }

        public bool FinishTask(string filename)
        {
            if (_taskFiles.Remove(filename))
            {
                if (_taskFiles.Count <= 0)
                {
                    EventHub.TriggerOnVictory();
                }
                EventHub.TriggerOnUploadFileComplete(filename);
                return true;    
            }

            return false;
        }
    }
}