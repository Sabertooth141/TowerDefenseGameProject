using System.Collections;
using Misc;
using Misc.Generator;
using UnityEngine;
using UnityEngine.AI;

namespace Entity.Enemy.EnemyPathfinding
{
    public class EnemyNavAgentController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Transform target;
        [SerializeField] private Transform player;

        private bool _isPathFinding;
        private GeneratorController _generatorController;
        private Vector3 _lastDestination;
        private bool _hasDestination;
        private Coroutine _knockbackRoutine;

        private void Awake()
        {
            if (target == null)
                target = GameObject.FindGameObjectWithTag("EnemyGoal").transform;

            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;

            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (agent == null)
                Debug.LogError("EnemyNavAgentController: NavMeshAgent missing");

            _generatorController = target.GetComponent<GeneratorController>();

            agent.speed = 7f;
            agent.acceleration = 30f;
            agent.angularSpeed = 1000f;
            agent.autoBraking = true;
        }

        void Update()
        {
            if (!_isPathFinding || player == null)
                return;

            Vector3 dest = _generatorController.IsGeneratorRunning
                ? target.position
                : player.position;

            if (!_hasDestination || (_lastDestination - dest).sqrMagnitude > 0.01f)
            {
                agent.SetDestination(dest);
                _lastDestination = dest;
                _hasDestination = true;
            }
        }

        public void ApplyKnockback(Vector3 direction, float force, float blendTime)
        {
            if (!agent || !agent.enabled || !agent.isOnNavMesh)
                return;

            if (_knockbackRoutine != null)
                StopCoroutine(_knockbackRoutine);

            _knockbackRoutine = StartCoroutine(KnockbackRoutine(direction, force, blendTime));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction, float force, float blendTime)
        {
            float t = 0f;

            Vector3 knockVelocity = direction.normalized * force;

            while (t < blendTime)
            {
                agent.velocity = Vector3.Lerp(knockVelocity, Vector3.zero, t / blendTime);
                t += Time.deltaTime;
                yield return null;
            }

            agent.velocity = Vector3.zero;
            _knockbackRoutine = null;
        }

        public void StartPathFinding()
        {
            if (!agent || !agent.enabled || !agent.isOnNavMesh)
                return;

            if (!_isPathFinding)
            {
                _isPathFinding = true;
                agent.isStopped = false;
            }
        }

        public void StopPathFinding()
        {
            if (!agent || !agent.enabled || !agent.isOnNavMesh)
                return;

            if (_isPathFinding)
            {
                _isPathFinding = false;
                agent.isStopped = true;
            }
        }

        public void SetStoppingDistance(float distance)
        {
            if (distance < 0)
                return;

            agent.stoppingDistance = distance;
        }
    }
}
