using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] Transform respawn;
    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out PlayerHealth pHealth);
        if (pHealth != null) pHealth.SetRespawn(respawn);
    }
}
