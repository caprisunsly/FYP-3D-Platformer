using DG.Tweening;
using System;
using UnityEngine;

public class Key : Pickup
{
    [SerializeField] string keyName;
    public static event Action<string> OnCollected;

    public override void CollectEvent()
    {
        OnCollected(keyName);
    }

    public override void CollectComplete()
    {
        Destroy(transform.parent.gameObject);
    }
}
