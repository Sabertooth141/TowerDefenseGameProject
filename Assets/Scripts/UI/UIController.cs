using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Turret;
using GameEvents;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private TextMeshProUGUI interactionText;
        [SerializeField] private TextMeshProUGUI availableTurretsText;
        [SerializeField] private TextMeshProUGUI currentTasksText;
        [SerializeField] private TextMeshProUGUI generatorMsgText;
        [SerializeField] private GameObject enemiesAlertText;
        [SerializeField] private String generatorUnderHackMsg;
        [SerializeField] private String generatorHackedMsg;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private GameObject hurtWarning;
        [SerializeField] private TextMeshProUGUI uploadCompleteText;

        private void Start()
        {
            InitUIComponents();
        }

        private void Awake()
        {
            // due to start order has to stay in awake
            EventHub.OnTurretUpdate += HandleTurretUpdate;
            EventHub.OnFilesGenerated += HandleTaskDisplay;
            EventHub.OnUploadFileComplete += HandleTaskDisplay;
            EventHub.OnPlayerControl += HandleHUDStart;
            EventHub.OnGeneratorStart += HandleGeneratorStart;
            EventHub.OnTryTurnOffGenerator += HandleTryTurnOffGenerator;
            EventHub.OnGeneratorTurnOff += HandleGeneratorTurnOff;
            EventHub.OnStopTurnOffGenerator += HandleGeneratorTurnOffStopped;
            EventHub.OnPlayerHurt += HandlePlayerHurt;
            EventHub.OnVictory += HandleVictory;

            CheckNull();
        }

        private void OnDestroy()
        {
            EventHub.OnTurretUpdate -= HandleTurretUpdate;
            EventHub.OnFilesGenerated -= HandleTaskDisplay;
            EventHub.OnUploadFileComplete -= HandleTaskDisplay;
            EventHub.OnPlayerControl -= HandleHUDStart;
            EventHub.OnGeneratorStart -= HandleGeneratorStart;
            EventHub.OnTryTurnOffGenerator -= HandleTryTurnOffGenerator;
            EventHub.OnGeneratorTurnOff -= HandleGeneratorTurnOff;
            EventHub.OnStopTurnOffGenerator -= HandleGeneratorTurnOffStopped;
            EventHub.OnPlayerHurt -= HandlePlayerHurt;
            EventHub.OnVictory -= HandleVictory;
        }

        private void CheckNull()
        {
            if (hudPanel == null)
            {
                Debug.LogError("PlayerController: GoalHpText not found");
            }

            if (interactionText == null)
            {
                Debug.LogError("PlayerController: InteractionText not found");
            }

            if (availableTurretsText == null)
            {
                Debug.LogError("PlayerController: AvailableTurretsText not found");
            }

            if (currentTasksText == null)
            {
                Debug.LogError("PlayerController: CurrentTasksText not found");
            }
        }

        private void InitUIComponents()
        {
            interactionText.text = "";
            generatorMsgText.text = "";
            enemiesAlertText.SetActive(false);
            hurtWarning.SetActive(false);
            uploadCompleteText.text = "";

            hudPanel.SetActive(false);
        }

        private void HandleTaskDisplay(string filename)
        {
            Dictionary<string, int> currentTasks = TaskManager.Instance.GetTaskFiles();
            currentTasksText.text = "";
            foreach (KeyValuePair<string, int> task in currentTasks)
            {
                currentTasksText.text += $"{task.Key}\n";
            }
            
            StartCoroutine(HandleUploadTextChange(filename));
        }
        
        private void HandleTaskDisplay()
        {
            Dictionary<string, int> currentTasks = TaskManager.Instance.GetTaskFiles();
            currentTasksText.text = "";
            foreach (KeyValuePair<string, int> task in currentTasks)
            {
                currentTasksText.text += $"{task.Key}\n";
            }
        }

        public void EnableInteraction(string interactMsg)
        {
            interactionText.text = interactMsg;
        }

        public void DisableInteraction()
        {
            interactionText.text = "";
        }

        public void HandleTurretUpdate(int availableTurret)
        {
            availableTurretsText.text = $"{availableTurret}";
        }

        public void HideHUDPanel()
        {
            hudPanel.SetActive(false);
        }

        public void ShowHUDPanel()
        {
            hudPanel.SetActive(true);
        }

        private void HandleHUDStart()
        {
            ShowHUDPanel();
        }

        private void HandleTryTurnOffGenerator()
        {
            generatorMsgText.text = generatorUnderHackMsg;
        }

        private void HandleGeneratorTurnOff()
        {
            generatorMsgText.text = generatorHackedMsg;
            enemiesAlertText.SetActive(false);
        }

        private void HandleGeneratorTurnOffStopped()
        {
            generatorMsgText.text = "";
        }

        private void HandleGeneratorStart()
        {
            generatorMsgText.text = "";
            enemiesAlertText.SetActive(true);
        }

        private void HandlePlayerHurt(float playerHp)
        {
            StartCoroutine(ChangeElements(playerHp));
        }

        private IEnumerator ChangeElements(float playerHp)
        {
            if (hurtWarning == null)
            {
                yield break;
            }

            if (playerHealthText == null)
            {
                yield break;
            }
            
            hurtWarning.SetActive(true);
            playerHealthText.text = "";
            yield return new WaitForSeconds(0.15f);
            playerHealthText.text = $"{playerHp}%";
            yield return new WaitForSeconds(0.15f);
            playerHealthText.text = "";
            hurtWarning.SetActive(false);
            yield return new WaitForSeconds(0.13f);
            playerHealthText.text = $"{playerHp}%";
            hurtWarning.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            hurtWarning.SetActive(false);
        }
        
        private IEnumerator HandleUploadTextChange(string filename)
        {
            if (filename == null)
            {
                yield break;
            }

            if (uploadCompleteText == null)
            {
                yield break;
            }
            
            uploadCompleteText.text = $"<{filename}> UPLOAD COMPLETE";
            yield return new WaitForSeconds(0.5f);
            uploadCompleteText.text = "";
            yield return new WaitForSeconds(0.2f);
            uploadCompleteText.text = $"<{filename}> UPLOAD COMPLETE";
            yield return new WaitForSeconds(0.5f);
            uploadCompleteText.text = "";
        }

        private void HandleVictory()
        {
            HideHUDPanel();
        }
    }
}