using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/Standing")]
public class State_Standing : Base_State
{
    public override void JumpStart()
    {
        sm.ChangeState(sm.stateJumping);
    }

    public override void GroundedEnd()
    {
        sm.ChangeState(sm.stateFalling);
    }

    public override void CrouchStart()
    {
        if (pc.rb.linearVelocity.magnitude > 0.5f && pc.gatedDir != Vector2.zero) sm.ChangeState(sm.stateSliding);
/*        else sm.ChangeState(sm.stateCrouching);*/
    }

    public override void TailSwipeStart()
    {
        sm.ChangeState(sm.stateTailSwipe);
    }
}
