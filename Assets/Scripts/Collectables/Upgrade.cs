using System;
using UnityEngine;

public class Upgrade : Pickup
{
    public static event Action<UpgradeType> OnCollectUpgrade;
    public UpgradeType upgradeType;

    public override void CollectEvent(GameObject player = null)
    {
        OnCollectUpgrade?.Invoke(upgradeType);
        base.CollectEvent(player);
    }
}
public enum UpgradeType
{
    None,
    WallKick,
    Slide
}
