using UnityEngine;
[System.Serializable]
public abstract class Base_State : ScriptableObject
{
    [Header("Base State")]
    public PlayerController pc { get; protected set; }
    public PlayerStateManager sm { get; protected set; }
    [field: SerializeField] public float transitionTime { get; protected set; }
    [field: SerializeField] public float speedMax { get; protected set; } = 400;
    [field: SerializeField] public float speedAccel { get; protected set; } = 150;
    [field: SerializeField] public float speedDecel { get; protected set; } = 150;
    [field: SerializeField] public float gravityMult { get; protected set; } = 1;
    [field: SerializeField] public bool shouldRotate { get; protected set; } = true;

    public virtual void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        pc = PC; sm = SM;
        pc.SetSpeed(speedMax, speedAccel, speedDecel, gravityMult, shouldRotate? 1:0);
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
    public virtual void DiveStart()
    {

    }

    public virtual void TailSwipeStart()
    {

    }
}
