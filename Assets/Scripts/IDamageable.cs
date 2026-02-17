using UnityEngine;

public interface  IDamageable
{
    public void Damage(int damage, Transform instigator, DamageTypes damageType = DamageTypes.Default);

    public enum DamageTypes
    {
        Default,
        LevelHazard,
        Healing
    }
}
