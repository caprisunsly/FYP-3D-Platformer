using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;

public class AIS_Wander : AIState
{
    [SerializeField] float range;
    public override bool CalculateTarget(out Vector3 target)
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1, NavMesh.AllAreas))
        {
            target =  hit.position;
            return true;
        }
        target =  Vector3.zero;
        return false;
    }

    public override void StateFixedUpdate()
    {
    }
}
