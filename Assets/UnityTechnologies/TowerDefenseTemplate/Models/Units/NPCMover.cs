using UnityEngine;
using UnityEngine.AI;

public class NPCMover : MonoBehaviour
{
    public Transform destino;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null || destino == null)
        {
            return;
        }

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        agent.isStopped = false;
        agent.SetDestination(destino.position);
    }
}
