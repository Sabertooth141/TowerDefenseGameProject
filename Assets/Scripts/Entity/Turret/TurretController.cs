using System;
using System.Collections.Generic;
using Entity.Turret.TurretStateMachine;
using EventSystem;
using UnityEngine;

namespace Entity.Turret
{
    public class TurretController : MonoBehaviour
    {
        public float rotationSpeed = 2.0f;
        public float timeToReset = 10.0f;
        public float firingAngle = 3.0f;
        public float loseLockAngle = 5.0f;
        public float lockOnTime = 0.5f;

        public Transform turretHead;
        public Transform turretFiringPoint;
        public GameObject turretProjectile;

        public bool HasTargets => targets.Count > 0;
        private float _resetTimer;

        [HideInInspector] public GameObject currTarget;
        [HideInInspector] public List<GameObject> targets;

        [HideInInspector] public TurretStateMachine.TurretStateMachine stateMachine;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            targets = new List<GameObject>();

            if (turretHead == null)
            {
                Debug.LogError("TurretHead field is missing");
            }

            if (turretFiringPoint == null)
            {
                Debug.LogError("TurretFiringPoint field is missing");
            }

            if (turretProjectile == null)
            {
                Debug.LogError("TurretProjectile field is missing");
            }

            stateMachine = new TurretStateMachine.TurretStateMachine();
            stateMachine.ChangeState(new TurretIdleState(this));
        }

        private void OnEnable()
        {
            EventHub.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            EventHub.OnEnemyDied -= HandleEnemyDied;
        }

        // Update is called once per frame
        void Update()
        {
            stateMachine.Update();
        }

        private void HandleEnemyDied(Entity entity)
        {
            targets.Remove(entity.gameObject);
            stateMachine.ChangeState(new TurretIdleState(this));
        }

        public void Fire()
        {
            Instantiate(turretProjectile, turretFiringPoint.position, turretFiringPoint.rotation);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                targets.Add(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Enemy"))
            {
                return;
            }

            targets.Remove(other.gameObject);
            stateMachine.ChangeState(new TurretIdleState(this));
        }
    }
}