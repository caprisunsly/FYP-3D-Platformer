using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerStateManager stateManager;
    PlayerController pc;
    bool gateJoystick;
    [SerializeField] float gamepadSens, mouseSens;
    [SerializeField] float gateAngle;
    [SerializeField] CinemachineInputAxisController axisController;

    private void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();
        pc = GetComponent<PlayerController>();
    }

    public void OnControlsChanged(PlayerInput input)
    {
        Debug.Log("changed! " + input.currentControlScheme);
        if (input.currentControlScheme == "XB") ;//change button prompts
        else if (input.currentControlScheme == "PS") ;
        else if (input.currentControlScheme == "NS") ;
        else
        {
            gateJoystick = false;
            SetSensitivity(mouseSens);
            return;
        }
        SetSensitivity(gamepadSens);
        gateJoystick = true;
    }

    void SetSensitivity(float gain)
    {
        axisController.Controllers[0].Input.Gain = gain;
        axisController.Controllers[1].Input.Gain = -gain;
    }

    public void OnMove(CallbackContext context)
    {
        Vector2 input = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1);
        if (gateJoystick) input = MoveInputProcessing(input);

        stateManager.currentState.Movement(input);
        pc.Movement(input, Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1));
    }

    Vector2 MoveInputProcessing(Vector2 input)
    {
        if (input.magnitude == 0) return input;
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

        //round the angle to gate
        angle = Mathf.Round(angle / gateAngle) * gateAngle;

        //cos/sin returns x/y on unit circle, round to get -1 -> 1 range
        float horizontalOut = Mathf.Round(Mathf.Cos(angle * Mathf.Deg2Rad));
        float verticalOut = Mathf.Round(Mathf.Sin(angle * Mathf.Deg2Rad));

        return Vector2.ClampMagnitude(new Vector2(horizontalOut, verticalOut), 1);
    }

    public void OnJump(CallbackContext context)
    {
        if (context.started)
        {
            pc.jumpHeld = true;
            stateManager.currentState.JumpStart();
        }
        if (context.canceled) 
        {
            pc.jumpHeld = false;
            stateManager.currentState.JumpCancel();
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

    public void OnAirDive(CallbackContext context)
    {
        if (context.started) stateManager.currentState.DiveStart();
    }
}
