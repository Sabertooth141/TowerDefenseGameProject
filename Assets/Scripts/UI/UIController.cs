using System;
using System.Collections.Generic;
using Entity.Turret;
using EventSystem;
using Misc;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private TextMeshProUGUI enemyGoalHpText;
        [SerializeField] private TextMeshProUGUI enemyGoalHpIndic;
        [SerializeField] private TextMeshProUGUI interactionText;
        [SerializeField] private TextMeshProUGUI availableTurretsText;
        [SerializeField] private TextMeshProUGUI currentTasksText;

        private void Start()
        {
            InitUIComponents();
        }

        private void Awake()
        {
            // due to start order has to stay in awake
            EventHub.OnGoalHurt += HandleGoalHurt;
            EventHub.OnTurretUpdate += HandleTurretUpdate;
            EventHub.OnFilesGenerated += HandleTaskDisplay;
            EventHub.OnUploadFileComplete += HandleTaskDisplay;
            CheckNull();
        }

        private void CheckNull()
        {
            if (hudPanel == null)
            {
                Debug.LogError("PlayerController: GoalHpText not found");
            }

            if (enemyGoalHpText == null)
            {
                Debug.LogError("PlayerController: GoalHpText not found");
            }

            if (enemyGoalHpIndic == null)
            {
                Debug.LogError("PlayerController: GoalHpIndic not found");
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
            if (enemyGoalHpIndic != null)
            {
                enemyGoalHpIndic.gameObject.SetActive(false);   
            }
            enemyGoalHpText.text = "";
            interactionText.text = "";
        }

        private void HandleTaskDisplay()
        {
            Dictionary<string, int> currentTasks = TaskManager.Instance.GetTaskFiles();
            currentTasksText.text = "";
            foreach (KeyValuePair<string, int> task in currentTasks)
            {
                currentTasksText.text +=  $"{task.Key}\n";
            }
        }

        private void HandleGoalHurt(float goalCurrHp, float goalMaxHp)
        {
            enemyGoalHpIndic.gameObject.SetActive(true);
            enemyGoalHpText.text = $"{goalCurrHp}/{goalMaxHp}";
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

        private void OnDestroy()
        {
            EventHub.OnGoalHurt -= HandleGoalHurt;
            EventHub.OnTurretUpdate -= HandleTurretUpdate;
        }
        
        public void HideHUDPanel()
        {
            hudPanel.SetActive(false);
        }

        public void ShowHUDPanel()
        {
            hudPanel.SetActive(true);
        }
    }
}