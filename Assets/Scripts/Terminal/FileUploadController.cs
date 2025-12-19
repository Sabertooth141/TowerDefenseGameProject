using System;
using System.Collections;
using System.Collections.Generic;
using EventSystem;
using Misc;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Terminal
{
    public class FileUploadController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TerminalController terminalController;
        [SerializeField] private TaskManager taskManager;

        [Header("Upload settings")]
        [SerializeField] private int progressBarLen = 40;

        private readonly WaitForSeconds _wait200ms = new WaitForSeconds(0.2f);
        private readonly WaitForSeconds _wait50ms = new WaitForSeconds(0.05f);

        private void Awake()
        {
            if (terminalController == null)
            {
                Debug.LogError("No terminal controller assigned!");
            }

            if (taskManager == null)
            {
                Debug.LogError("No task manager assigned!");
            }
        }

        public void StartVerification(string[] args)
        {
            StartCoroutine(Verification(args));
        }

        private void AddOutput(string output)
        {
            terminalController.AddOutput(output);
        }

        private IEnumerator Verification(string[] args)
        {
            // client REQ
            AddOutput("CLIENT: Requesting upload permission...");
            AddOutput("---------------------------------------------");
            yield return _wait200ms;
            int winID = Random.Range(20000, 65535);
            string[] reqMsg = GenerateReq(winID);
            foreach (var line in reqMsg)
            {
                AddOutput(line);
                yield return _wait50ms;
            }

            AddOutput("---------------------------------------------");
            yield return new WaitForSeconds(Random.Range(0.4f, 1.0f));

            // server RES
            AddOutput("SERVER: Receiving challenge message...");
            yield return _wait200ms;
            AddOutput("---------------------------------------------");
            string[] serverChallenge = taskManager.SendChallengeMsg(winID);

            foreach (var line in serverChallenge)
            {
                AddOutput(line);
                yield return _wait50ms;
            }

            AddOutput("---------------------------------------------");
            yield return _wait200ms;

            // client VERIFY
            AddOutput("CLIENT: Generating response to challenge...");
            yield return _wait200ms;
            string filename = args[1];
            string[] resMsg = GenerateChallengeResponse(filename, winID);
            foreach (var line in resMsg)
            {
                AddOutput(line);
                yield return _wait50ms;
            }

            AddOutput("---------------------------------------------");
            yield return new WaitForSeconds(Random.Range(0.4f, 1.0f));

            // server RES
            string[] filenameVerifyMsg;
            bool verifyFlag = taskManager.VerifyFilename(filename, winID, out filenameVerifyMsg);
            AddOutput("SERVER: Receiving verification...");
            yield return _wait200ms;
            foreach (var line in filenameVerifyMsg)
            {
                AddOutput(line);
                yield return _wait50ms;
            }

            AddOutput("---------------------------------------------");
            yield return _wait200ms;

            // client process server RES
            if (!verifyFlag)
            {
                AddOutput("CLIENT: Request INVALID, terminating connection...");
            }
            else
            {
                AddOutput("CLIENT: Request OK, initiating upload...");
                AddOutput("---------------------------------------------");
                KeyValuePair<string, int> targetFile = TaskManager.Instance.GetFileByName(filename);
                StartCoroutine(Upload(filename, targetFile.Value));
            }
        }

        private IEnumerator Upload(string filename, int fileSize)
        {
            if (fileSize <= 0)
            {
                yield break;
            }

            AddOutput($"Uploading: {filename}");
            AddOutput($"Size: {fileSize}KB");
            AddOutput("Initializing...");
            AddOutput("---------------------------------------------");
            AddOutput(""); // placeholder for progress bar
            AddOutput(""); // placeholder for stats
            int progressBarIndex = terminalController.GetCurrentLineIndex() - 1;
            int uploadStatsIndex = progressBarIndex + 1;

            yield return Random.Range(0.2f, 1.0f);
            float progress = 0.0f;

            while (progress < fileSize)
            {
                float increment = Random.Range(10f, 100f);
                progress += increment;
                progress = Mathf.Min(progress, fileSize); // Don't exceed fileSize
                
                float speed = increment / 0.2f; // KB per second (since we wait 0.2s)

                // Calculate percentage for progress bar
                float percentage = progress / fileSize;
                int filled = Mathf.RoundToInt(progressBarLen * percentage);
                string bar = "[" + new string('█', filled) + new string('░', progressBarLen - filled) + "]";

                int percentDisplay = Mathf.RoundToInt(percentage * 100);
                int uploaded = Mathf.RoundToInt(progress);
        
                terminalController.UpdateOutputLine(progressBarIndex, $"{bar} {percentDisplay}%");
                terminalController.UpdateOutputLine(uploadStatsIndex, $"{uploaded}/{fileSize} KB | Speed: {speed:F1} KB/s");

                yield return _wait200ms;
            }
            
            AddOutput("---------------------------------------------");
            AddOutput("CLIENT: File upload complete");
            taskManager.FinishTask(filename);
            EventHub.TriggerOnCommandCompleted();
        }

        private string[] GenerateReq(int winID)
        {
            return new[]
            {
                "[CLIENT -> SERVER]:",
                "flag: SYN",
                "type: <upload>",
                $"WIN: {winID}",
                $"session-id: sess_{name + GetInstanceID()}",
                $"session-time: {Time.deltaTime}"
            };
        }

        private string[] GenerateChallengeResponse(string filename, int winID)
        {
            return new[]
            {
                "[CLIENT -> SERVER]:",
                "flag: ACK",
                "type: <upload-verify>",
                $"WIN: {winID}",
                $"file-name: {filename}",
                $"session-id: sess_{name + GetInstanceID()}",
                $"session-time: {Time.deltaTime}"
            };
        }
    }
}