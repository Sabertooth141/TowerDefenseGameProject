using System;
using Entity.Turret;
using EventSystem;
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

        private void Start()
        {
            CheckNull();
            InitUIComponents();
        }

        private void Awake()
        {
            EventHub.OnGoalHurt += HandleGoalHurt;
            EventHub.OnTurretUpdate += HandleTurretUpdate;
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
    }
}