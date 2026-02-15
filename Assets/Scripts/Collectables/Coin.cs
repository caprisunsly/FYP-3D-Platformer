using System;
using UnityEngine;

public class Coin : Pickup
{
    public static event Action OnCollected;
    public override void CollectEvent()
    {
        OnCollected();
    }
}
