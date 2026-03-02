using UnityEngine;

public class Health : Pickup
{
    SphereCollider col;
    [SerializeField] MeshRenderer mr;
    [SerializeField] Material inactive, active;

    private void OnEnable()
    {
        col = GetComponent<SphereCollider>();
        PlayerHealth.HealthAtMax += ToggleHealthActive;
    }

    private void OnDisable()
    {
        PlayerHealth.HealthAtMax -= ToggleHealthActive;
    }

    void ToggleHealthActive(bool state)
    {
        col.enabled = !state;
        if (state) mr.material = inactive;
        else mr.material = active;
    }

    public override void CollectEvent(GameObject player = null)
    {
        PlayerHealth.HealthAtMax -= ToggleHealthActive;

        player.TryGetComponent(out IDamageable health);
        if (health != null) health.Damage(-1, null, IDamageable.DamageTypes.Healing);
        base.CollectEvent(player);
    }
}
