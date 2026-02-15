using UnityEngine;
using UnityEngine.AI;

public class AIS_Wander : AIState
{
    [SerializeField] float range;
    [SerializeField] Vector3 wanderCentre;
    [SerializeField] bool fixedWander = true;

    private void Awake()
    {
        wanderCentre = transform.position;
    }
    public override bool CalculateTarget(out Vector3 target)
    {
        if (!fixedWander) wanderCentre = transform.position;
        Vector3 randomPoint = wanderCentre + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1, NavMesh.AllAreas))
        {
            target =  hit.position;
            return true;
        }
        target =  Vector3.zero;
        return false;
    }

    public override void StateUpdate()
    {
        if (manager.agent.enabled && manager.agent.remainingDistance <= manager.agent.stoppingDistance && !manager.pathing)
        {
            StartCoroutine(manager.CalculateTarget());
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (Application.isPlaying) Gizmos.DrawWireSphere(wanderCentre, range);
        else Gizmos.DrawWireSphere(wanderCentre + transform.position, range);
    }
}
