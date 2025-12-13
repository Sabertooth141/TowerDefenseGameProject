using System;
using System.Collections.Generic;
using Entity.Turret.TurretStateMachine;
using EventSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Entity.Turret
{
    public class TurretController : MonoBehaviour
    {
        [Header("Turret Settings")]
        public LayerMask losMask;
        public float rotationSpeed = 2.0f;
        public float timeToReset = 10.0f;
        public float firingAngle = 3.0f;
        public float loseLockAngle = 5.0f;
        public float lockOnTime = 0.5f; 
        public float damage = 50.0f;

        [Header("References")]
        public Transform turretHead;
        public Transform turretFiringPoint;
        public SphereCollider detectionCollider;
        public bool HasTargets => targets.Count > 0;

        private float _resetTimer;
        private List<GameObject> _targetsToCheck;
        private float _range;

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

            if (detectionCollider == null)
            {
                Debug.LogError("DetectionCollider field is missing");
            }

            stateMachine = new TurretStateMachine.TurretStateMachine();
            stateMachine.ChangeState(new TurretIdleState(this));

            _range = detectionCollider.radius;
            TurretManager.Instance.RegisterTurret(gameObject);
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
            if (_targetsToCheck.Contains(entity.gameObject))
            {
                _targetsToCheck.Remove(entity.gameObject);   
            }

            if (targets.Contains(entity.gameObject))
            {
                targets.Remove(entity.gameObject);
            }
            
            currTarget = null;
            // stateMachine.ChangeState(new TurretIdleState(this));
        }

        public void Fire()
        {
            Ray bulletRay = new Ray(turretFiringPoint.position, turretFiringPoint.forward);
            
            if (Physics.Raycast(bulletRay, out RaycastHit hit, _range))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    hit.transform.gameObject.GetComponent<Entity>().TakeDamage(damage);
                }
            }
        }

        private bool LOSDetection(Transform target)
        {
            int maskToIgnore = ~losMask;
            Vector3 direction = (target.position - turretFiringPoint.position).normalized;
            
            if (Physics.Raycast(turretFiringPoint.position, direction, out RaycastHit hit, Mathf.Infinity,
                    maskToIgnore))
            {
                if (hit.transform == target)
                {
                    Debug.DrawRay(turretFiringPoint.position, direction * 100f, Color.azure);
                    return true;
                }

                Debug.DrawRay(turretFiringPoint.position, direction * 100f, Color.red);
                return false;

                // return hit.transform == target;
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