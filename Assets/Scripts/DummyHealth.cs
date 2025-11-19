using UnityEngine;

public class DummyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth;
    int currentHealth;
    [SerializeField] Animator anim;

    public void Damage(int damage, Transform instigator)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Death();
    }

    public void Death()
    {
        anim.SetTrigger("Death");
    }
}
