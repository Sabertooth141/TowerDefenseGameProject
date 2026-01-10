using System;
using System.Collections;
using System.Collections.Generic;
using GameEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class ExtractionPanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private UISFXController sfxController;
        [SerializeField] private GameObject victoryPanel;
        
        [Header("Terminal settings")]
        public int maxOutputLines = 50;

        private readonly List<string> _outputLines = new();
        
        private void Awake()
        {
            victoryPanel.SetActive(false);
        }

        private void Start()
        {
            EventHub.OnVictory += PlayVictoryMessages;
        }

        private void OnDestroy()
        {
            EventHub.OnVictory -= PlayVictoryMessages;
        }

        public void PlayVictoryMessages()
        {
            victoryPanel.SetActive(true);
            StartCoroutine(StartMsgs());
        }

        private IEnumerator StartMsgs()
        {
            string[] lines =
            {
                ">> INBOUND SIGNAL DETECTED",
                ">> EXTERNAL DRONE RESPONSE CONFIRMED",
                "",
                "[CTRL] ESTABLISHING RETURN CHANNEL",
                "[CTRL] QUERYING UNIT STATUS",
                "",
                "[DRONE] ID ................. DRONE-07",
                $"[DRONE] SIGNAL LEVEL ....... {Random.Range(40f, 100f)}%",
                "[DRONE] FLIGHT CONTROL ..... STABLE",
                "[DRONE] NAV SYSTEM ......... LOCKED",
                "",
                "[CTRL] RECEIVING EXTRACTION CONFIRMATION",
                "[DRONE] PAYLOAD SECURED",
                "[DRONE] EGRESS VECTOR SET",
                "",
                "[DRONE] HOSTILE ZONE CLEARED",
                "[CTRL] INITIATING RETURN-TO-BASE ROUTINE",
                "",
                "[SYS] LOCAL CACHE SEALED",
                "[SYS] TEMPORARY STORAGE LOCKED",
                "[SYS] SESSION KEYS ROTATED",
                "",
                "[SYS] PREPARING DATA HANDOFF",
                "[SYS] SWITCHING TO POST-OPERATION MODE",
                "",
                "[SYS] READY TO RE-ESTABLISH UPLINK"
            };

            foreach (var line in lines)
            {
                AddOutput(line);
                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            }
            
            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
            UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
        }
        
        public void AddOutput(string output)
        {
            sfxController.PlayTypingSFX();
            if (_outputLines.Count >= maxOutputLines)
                _outputLines.RemoveAt(0);

            _outputLines.Add(output);
            outputText.text = string.Join("\n", _outputLines);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
