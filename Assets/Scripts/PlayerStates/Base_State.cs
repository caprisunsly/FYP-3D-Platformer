using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class Base_State : MonoBehaviour
{
    public CharacterController controller;
    public PlayerStateManager stateManager;

    public Base_State(CharacterController controller, PlayerStateManager stateManager)
    {
        this.controller = controller;
        this.stateManager = stateManager;
    }

    public virtual void StateEntry()
    {

    }

    public virtual void StateUpdate()
    {

    }

    public virtual void StateFixedUpdate()
    {

    }

    public virtual void StateExit()
    {

    }

    /*
    public override void StateEntry()
    {
        base.StateEntry();

    }

    public override void StateUpdate()
    {
        base.StateUpdate();

    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

    }

    public override void StateExit()
    {
        base.StateExit();

    }
*/
}
