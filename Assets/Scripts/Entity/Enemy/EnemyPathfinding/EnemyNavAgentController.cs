using System;
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

        private void Awake()
        {
            if (target == null)
            {
                target = GameObject.FindGameObjectWithTag("EnemyGoal").transform;
            }

            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (agent == null)
            {
                Debug.LogError("EnemyNavAgentController: NavMeshAgent missing");
            }
            
            _generatorController = target.GetComponent<GeneratorController>();

            agent.speed = 7f;
            agent.acceleration = 30f;
            agent.angularSpeed = 1000f;
            agent.autoBraking = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isPathFinding)
            {
                return;
            }
            
            if (player == null)
            {
                return;
            }

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

        public void StartPathFinding()
        {
            if (!_isPathFinding)
            {
                _isPathFinding = true;
                agent.isStopped = false;
            }
        }

        public void StopPathFinding()
        {
            if (_isPathFinding)
            {
                _isPathFinding = false;
                agent.isStopped = true;
            }
        }

        public void SetStoppingDistance(float distance)
        {
            if (distance < 0)
            {
                return;
            }
            agent.stoppingDistance = distance;
        }

    }
}