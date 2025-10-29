using UnityEngine;

public abstract class Base_State : MonoBehaviour
{
    public PlayerController pc { get; protected set; }
    public PlayerStateManager sm { get; protected set; }
    [field: SerializeField] public float transitionTime { get; protected set; }

    private void Start()
    {
        pc = GetComponent<PlayerController>();
        sm = GetComponent<PlayerStateManager>();
    }

    public virtual void StateEntry()
    {

    }

    public virtual void StateUpdate()
    {
        StateLogic();
    }

    public virtual void StateFixedUpdate()
    {

    }

    public virtual void StateExit()
    {

    }

    public virtual void StateLogic()
    {

    }

    public virtual void Movement(Vector2 dir)
    {

    }

    public virtual void JumpStart()
    {

    }

    public virtual void JumpCancel()
    {

    }

    public virtual void CrouchStart()
    {

    }

    public virtual void CrouchCancel()
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
