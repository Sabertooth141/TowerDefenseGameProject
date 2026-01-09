using System;
using UnityEngine;

namespace Entity.Enemy
{
    public class HackingTriggerController : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyController;

        private void Awake()
        {
            if (enemyController == null)
            {
                Debug.LogError("Hacking Trigger Controller: enemyController not found");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            enemyController.HandleHackingTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            enemyController.HandleHackingTriggerExit(other);
        }
    }
}
