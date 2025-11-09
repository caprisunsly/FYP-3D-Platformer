using UnityEngine;

public abstract class Base_State : MonoBehaviour
{
    [Header("Base State")]
    public PlayerController pc { get; protected set; }
    public PlayerStateManager sm { get; protected set; }
    [field: SerializeField] public float transitionTime { get; protected set; }
    [field: SerializeField] public float speedMax { get; protected set; } = 400;
    [field: SerializeField] public float speedAccel { get; protected set; } = 150;
    [field: SerializeField] public float speedDecel { get; protected set; } = 150;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        sm = GetComponent<PlayerStateManager>();
    }

    public virtual void StateEntry()
    {
        pc.SetSpeed(speedMax, speedAccel, speedDecel);
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

    public virtual void GroundedStart()
    {

    }

    public virtual void GroundedEnd()
    {

    }
}
