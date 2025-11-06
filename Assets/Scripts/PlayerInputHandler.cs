using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerStateManager stateManager;
    PlayerController pc;

    private void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();
        pc = GetComponent<PlayerController>();
    }

    public void OnMove(CallbackContext context)
    {
        stateManager.currentState.Movement(context.ReadValue<Vector2>());
        pc.Movement(context.ReadValue<Vector2>());
    }

    public void OnJump(CallbackContext context)
    {
        if (context.started)
        {
            stateManager.currentState.JumpStart();
            pc.jumpHeld = true;
        }
        if (context.canceled) 
        {
            stateManager.currentState.JumpCancel();
            pc.jumpHeld = false;
        }
    }

    public void OnCrouch(CallbackContext context)
    {
        if (context.started)
        {
            stateManager.currentState.CrouchStart();
            pc.crouchHeld = true;
        }
        if (context.canceled)
        {
            stateManager.currentState.CrouchCancel();
            pc.crouchHeld = false;
        }
    }

    public void OnShift(CallbackContext context)
    {

    }
}
