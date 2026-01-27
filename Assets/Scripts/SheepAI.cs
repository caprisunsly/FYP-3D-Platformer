using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class SheepAI : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] float range;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    bool RandomWander(Vector3 centre, float range, out Vector3 result)
    {
        Vector3 randomPoint = centre + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}
