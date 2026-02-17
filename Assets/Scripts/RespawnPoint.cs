using System;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] Transform respawn;
    public static event Action<Transform> RespawnSet;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) RespawnSet?.Invoke(respawn);
    }
}
