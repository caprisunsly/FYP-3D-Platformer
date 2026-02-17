using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth;
    int currentHealth;

    public void Damage(int damage, Transform instigator, IDamageable.DamageTypes damageType)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Death();

        //play damage popup thing for cool quirky feedback
    }

    public void Death()
    {
        //disable all colliders and behaviours
        //play death animation -> destroy gameobject
    }
}
