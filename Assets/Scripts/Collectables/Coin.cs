using System;
using System.Collections;
using UnityEngine;

public class Coin : Pickup
{
    public static event Action OnCollected;
    public override void CollectEvent(GameObject player = null)
    {
        OnCollected();
    }
}
