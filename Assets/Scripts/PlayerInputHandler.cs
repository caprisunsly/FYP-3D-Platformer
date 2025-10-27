using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerController controller;
    MoveModuleManager manager;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        manager = GetComponent<MoveModuleManager>();
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
            manager.SpaceStart();
        }
        if (context.canceled) 
        {
            controller.JumpCancel();
            manager.SpaceStop();
        }
    }

    public void OnCrouch(CallbackContext context)
    {
        if (context.started) manager.CtrlStart();
        if (context.canceled) manager.CtrlStop();
    }

    public void OnShift(CallbackContext context)
    {
        if (context.started) manager.ShiftStart();
        if (context.canceled) manager.ShiftStop();
    }

    public void OnSpace(CallbackContext context)
    {
        if (context.started) manager.ShiftStart();
        if (context.canceled) manager.ShiftStop();
    }
}
