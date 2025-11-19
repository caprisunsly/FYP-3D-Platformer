using UnityEngine;

public class DamageBox : MonoBehaviour
{
    int damage;
    Transform parent;

    void InitializeDamageBox(int d, Transform p)
    {
        damage = d;
        parent = p;
    }

    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out IDamageable target);
        if (target != null) target.Damage(damage, parent);
    }
}
