using System;
using Entity.Enemy.EnemyPathfinding;
using Entity.Player;
using GameEvents;
using Misc;
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
        private bool _inAttackCooldown;
        private GeneratorController _generator;

        protected override void Start()
        {
            base.Start();

            EnemyManager.Instance.RegisterEnemy(gameObject);
            _navMeshAgent.SetStoppingDistance(1);
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<EnemyNavAgentController>();
            _navMeshAgent.StartPathFinding();

            _isActive = true;
            _isAttacking = false;

            if (_navMeshAgent == null)
            {
                Debug.LogError("EnemyController: NavMeshAgent is null");
            }

            if (sphereCollider == null)
            {
                Debug.LogError("EnemyController: Sphere Collider is null");
            }
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

            if (!_inAttackCooldown)
            {
                _navMeshAgent.StopPathFinding();
                _attackTarget.TakeDamage(damage);
                _inAttackCooldown = true;
            }

            if (_attackTimer > attackCooldown)
            {
                _attackTimer = 0;
                _navMeshAgent.StartPathFinding();
                _inAttackCooldown = false;
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

            if (_generator != null)
            {
                _generator.UnregisterHacker(gameObject);
                _generator = null;
            }
        }

        public void HandleHackingTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && !SceneManager.Instance.isGeneratorOn)
            {
                _attackTarget = other.gameObject.GetComponentInParent<PlayerController>();
                _attackTimer = 0;
                _inAttackCooldown = false;
                _isAttacking = true;
            }

            if (other.gameObject.CompareTag("EnemyGoal"))
            {
                _generator = other.gameObject.GetComponent<GeneratorController>();
                if (_generator.IsGeneratorRunning)
                {
                    _generator.RegisterHacker(gameObject);
                }
            }
        }

        public void HandleHackingTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && _attackTarget != null)
            {
                _attackTarget = null;
                _isAttacking = false;
                _navMeshAgent.StartPathFinding();
            }

            if (other.gameObject.CompareTag("EnemyGoal") && _generator != null)
            {
                _generator.UnregisterHacker(gameObject);
                _generator = null;
            }
        }
    }
}