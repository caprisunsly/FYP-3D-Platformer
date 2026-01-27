using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent OnInteract;


    private void OnTriggerEnter(Collider other)
    {
/*        other.TryGetComponent(out IDamageable target);
        if (target != null) target.Damage(damage, parent);*/
    }
}
