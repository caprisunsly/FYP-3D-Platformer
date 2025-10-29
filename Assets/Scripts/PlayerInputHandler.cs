using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerController controller;
    MoveSlide slide;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        slide = GetComponent<MoveSlide>();
    }

    public void OnMove(CallbackContext context)
    {
        controller.Movement(context.ReadValue<Vector2>());
    }

    public void OnJump(CallbackContext context)
    {
        if (context.started)
        {
            controller.JumpStart();
        }
        if (context.canceled) 
        {
            controller.JumpCancel();
        }
    }

    public void OnCrouch(CallbackContext context)
    {
        if (context.started) slide.InputStarted();
        if (context.canceled) slide.InputCancelled();
    }

    public void OnShift(CallbackContext context)
    {

    }

    public void OnSpace(CallbackContext context)
    {

    }
}
