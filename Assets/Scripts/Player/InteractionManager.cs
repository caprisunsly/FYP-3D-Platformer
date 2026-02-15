using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    Interactable interactable;


    private void OnEnable()
    {
        Interactable.OnInteractableDisabled += ClearInteractable;
    }

    private void OnDisable()
    {
        Interactable.OnInteractableDisabled -= ClearInteractable;
    }

    public void SetInteractable(Interactable i)
    {
        if (interactable != null) interactable.UnsetSelected();
        interactable = i;
    }

    public void ClearInteractable(Interactable i)
    {
        if (interactable == i) interactable = null;
    }

    public void Interact()
    {
        if (interactable != null) interactable.Interact();
    }

    public void InteractionComplete()
    {

    }
}
