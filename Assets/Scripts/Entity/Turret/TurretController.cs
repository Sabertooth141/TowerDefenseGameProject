using System;
using System.Collections.Generic;
using Entity.Turret.TurretStateMachine;
using EventSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.Turret
{
    public class TurretController : MonoBehaviour
    {
        public LayerMask losMask;
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
        private List<GameObject> _targetsToCheck;

        [HideInInspector] public GameObject currTarget;
        [HideInInspector] public List<GameObject> targets;

        [HideInInspector] public TurretStateMachine.TurretStateMachine stateMachine;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            targets = new List<GameObject>();
            _targetsToCheck = new List<GameObject>();

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

        private void FixedUpdate()
        {
            GetTargets();
        }

        private void HandleEnemyDied(Entity entity)
        {
            _targetsToCheck.Remove(entity.gameObject);
            targets.Remove(entity.gameObject);
            currTarget = null;
            // stateMachine.ChangeState(new TurretIdleState(this));
        }

        public void Fire()
        {
            Instantiate(turretProjectile, turretFiringPoint.position, turretFiringPoint.rotation);
        }

        private bool LOSDetection(Transform target)
        {
            int maskToIgnore = ~losMask;
            Vector3 direction = (target.position - turretFiringPoint.position).normalized;

            Debug.DrawRay(turretFiringPoint.position, direction * 100f, Color.red);
            if (Physics.Raycast(turretFiringPoint.position, direction, out RaycastHit hit, Mathf.Infinity,
                    maskToIgnore))
            {
                return hit.transform == target;
            }

            return false;
        }

        private void GetTargets()
        {
            if (_targetsToCheck.Count <= 0)
            {
                return;
            }
            
            _targetsToCheck.RemoveAll(t => t == null);
            targets.RemoveAll(t => t == null);
            foreach (var target in _targetsToCheck)
            {
                if (LOSDetection(target.transform))
                {
                    if (!targets.Contains(target))    
                        targets.Add(target);
                }
                else
                {
                    targets.Remove(target.gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                _targetsToCheck.Add(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Enemy"))
            {
                return;
            }

            _targetsToCheck.Remove(other.gameObject);
            targets.Remove(other.gameObject);
        }
    }
}