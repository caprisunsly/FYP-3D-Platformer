using UnityEngine;
using UnityEngine.AI;

public class AIS_Chase : AIState
{
    [HideInInspector] public Transform player;
    public override bool CalculateTarget(out Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(player.position, out hit, 20, NavMesh.AllAreas))
        {
            target = hit.position;
            return true;
        }
        target = Vector3.zero;
        return false;
    }

    public override void StateUpdate()
    {
        if (manager.agent.enabled) StartCoroutine(manager.CalculateTarget());
    }

}
