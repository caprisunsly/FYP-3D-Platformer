using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class Key : Pickup
{
    public static event Action OnCollected;

    void Awake()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(scale, 2);
    }

    public override void CollectEvent(GameObject player = null)
    {
        OnCollected?.Invoke();
        base.CollectEvent(player);
    }
}
