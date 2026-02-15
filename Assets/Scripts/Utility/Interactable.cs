using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent OnInteract;
    public static event Action<Interactable> OnInteractableDisabled;
    [SerializeField] GameObject selectedPopup;
    [SerializeField] Animator anim;

    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out InteractionManager target);
        if (target != null && target.CompareTag("Player")) target.SetInteractable(this);
        SetSelected();
    }

    private void OnTriggerExit(Collider other)
    {
        other.TryGetComponent(out InteractionManager target);
        if (target != null && target.CompareTag("Player")) target.ClearInteractable(this);
        UnsetSelected();
    }

    private void OnDisable()
    {
        OnInteractableDisabled?.Invoke(this);
        UnsetSelected();
    }

    public void SetSelected()
    {
        selectedPopup.SetActive(true);
    }

    public void UnsetSelected()
    {
        selectedPopup.SetActive(false);
    }

    public void Interact()
    {
        OnInteract?.Invoke();
        if (anim != null) anim.SetTrigger("Interact");
    }
}
