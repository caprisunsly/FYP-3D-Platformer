using System;
using UnityEngine;

public class Upgrade : Key
{
    public static event Action<UpgradeType> OnCollectUpgrade;
    public UpgradeType upgradeType;

    public override void CollectEvent(GameObject player = null)
    {
        OnCollectUpgrade?.Invoke(upgradeType);
        base.CollectEvent();
    }
}
public enum UpgradeType
{
    None,
    WallKick,
    Slide
}
