using UnityEngine;

public class LevelHazard : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable d);
        if (d != null)
        {
            d.Damage(1, transform, IDamageable.DamageTypes.LevelHazard);
        }
    }
}
