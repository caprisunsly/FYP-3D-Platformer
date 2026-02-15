using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public UnityEvent TriggerEnter;
    public UnityEvent TriggerExit;
    public event Action<Vector3> TriggerEnterPos;
    [NaughtyAttributes.Tag]
    public string Tag;

    private void OnTriggerEnter(Collider other)
    {
        if ((Tag == "" || other.CompareTag(Tag)))
        {
            if (TriggerEnter == null) return;
            TriggerEnter?.Invoke();
            TriggerEnterPos?.Invoke(other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Tag == "" || other.CompareTag(Tag))
        {
            if (TriggerExit == null) return;
            TriggerExit?.Invoke();
        }
    }
}
