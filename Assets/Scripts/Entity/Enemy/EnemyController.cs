using System;
using Entity.Enemy.EnemyPathfinding;
using EventSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

namespace Entity.Enemy
{
    public class EnemyController : Entity
    {
        [Header("Enemy Settings")]
        public float damage = 20.0f;
        public float attackCooldown = 2.0f;

        [Header("References")]
        [SerializeField] private SphereCollider sphereCollider;

        private bool _isAttacking;
        private bool _isActive;
        
        private EnemyNavAgentController _navMeshAgent;
        private float _attackTimer;
        private Entity _attackTarget;

        protected override void Start()
        {
            base.Start();
            
            if (_navMeshAgent == null)
            {
                Debug.LogError("EnemyController: NavMeshAgent is null");
            }

            if (sphereCollider == null)
            {
                Debug.LogError("EnemyController: Sphere Collider is null");
            }

            EnemyManager.Instance.RegisterEnemy(gameObject);
            _navMeshAgent.SetStoppingDistance(sphereCollider.radius);
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<EnemyNavAgentController>();
            _navMeshAgent.StartPathFinding();
            
            _isActive = true;
        }

        // Update is called once per frame
        protected override void Update()
        {
            if (!_isActive)
            {
                return;
            }
            
            base.Update();

            if (_isAttacking)
            {
                Attack();
            }
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
        }

        private void Attack()
        {
            if (_attackTarget == null)
            {
                return;
            }
            
            if (_attackTimer > attackCooldown)
            {
                _attackTimer = 0;
                _attackTarget.TakeDamage(damage);
            }

            _attackTimer += Time.deltaTime;
        }

        protected override void Die()
        {
            EventHub.TriggerEnemyDied(this);
            base.Die();
        }

        private void OnDestroy()
        {
            EnemyManager.Instance?.UnregisterEnemy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("EnemyGoal"))
            {
                _attackTarget = other.gameObject.GetComponent<Entity>();
                _isAttacking = true;

                if (_attackTarget == null)
                {
                    Debug.LogError("Attack Target is null");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ( _attackTarget != null && other.gameObject == _attackTarget.gameObject)
            {
                _attackTarget = null;
                _isAttacking = false;
            }
        }
    }
}