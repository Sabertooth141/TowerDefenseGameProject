using UnityEngine;
using UnityEngine.AI;

namespace Entity.Enemy.EnemyPathfinding
{
    public class EnemyNavAgentController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Transform target;

        private bool _isPathFinding;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (target == null)
            {
                target = GameObject.FindGameObjectWithTag("EnemyGoal").transform;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null && _isPathFinding)
            {
                agent.SetDestination(target.position);
            }
        }

        public void StartPathFinding()
        {
            if (!_isPathFinding)
            {
                _isPathFinding = true;
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