using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerMovement playerMovement;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void OnMove(CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        playerMovement.Move(new Vector3(input.x, 0, input.y));
    }

    public void OnLook(CallbackContext context)
    {
        //process & send input
    }

    public void OnJump(CallbackContext context)
    {
        if (context.performed) playerMovement.JumpStart();
        else if (context.canceled) playerMovement.JumpEnd();
    }
}
