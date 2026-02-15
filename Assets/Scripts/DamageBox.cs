using UnityEngine;

public class DamageBox : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] Transform parent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        other.TryGetComponent(out IDamageable target);
        if (target != null) target.Damage(damage, parent);
    }
}
