using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerController controller;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    public void OnMove(CallbackContext context)
    {
        controller.Movement(context.ReadValue<Vector2>());
    }

    public void OnJump(CallbackContext context)
    {
        if (context.started) controller.JumpStart();
        if (context.canceled) controller.JumpCancel();
    }
}
