using UnityEngine;
using UnityEngine.AI;

public class EnemyNavAgentController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform target;
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
        agent.SetDestination(target.position);
    }
}
