using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    Transform respawnPoint;
    public void Damage(int damage, Transform instigator)
    {
        if (instigator.CompareTag("LevelHazard"))
        {
            PerformRespawn();
        }
    }

    public void SetRespawn(Transform respawn)
    {
        respawnPoint = respawn;
    }

    public void PerformRespawn()
    {
        //do a cool quirky animation
        //fade the screen out
        transform.position = respawnPoint.position;
        //fade the screen in
        //play a different, cool quirky animation
    }
}
