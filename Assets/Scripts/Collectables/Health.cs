using UnityEngine;

public class Health : Pickup
{
    public override void CollectEvent(GameObject player = null)
    {
        player.TryGetComponent(out IDamageable health);
        if (health != null) health.Damage(-1, null, IDamageable.DamageTypes.Healing);
        base.CollectEvent(player);
    }
}
