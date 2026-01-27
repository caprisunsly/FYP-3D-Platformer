using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/Falling")]
public class State_Falling : Base_State
{
    [Header("Falling")]
    Coroutine c_jumpBuffer;
    [SerializeField] float jumpBufferTime;
    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;
    [SerializeField] LedgeCastData ledgeData;

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        ledgeData = new LedgeCastData(pc, ledgeHeight, ledgeSnapDistance, minimumHeightFromGround);
        pc.modelAnim.SetBool("Fall", true);
        if (pc.jumpedFrom == 0) pc.jumpedFrom = pc.transform.position.y;
    }

    public override void StateExit()
    {
        base.StateExit();
        pc.modelAnim.SetBool("Fall", false);
        pc.multiplier = 1f; 
    }

    IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
         c_jumpBuffer = null;
    }

    public override void StateFixedUpdate()
    {
        if (pc.grounded) sm.ChangeState(sm.stateStanding); //prevents edge case where players jump but get stuck on geometry and don't leave the ground
        if (LedgeCast.Check(ledgeData, pc.ungatedDir)) sm.ChangeState(sm.stateLedgeHang);
        if (pc.floorClose) pc.multiplier = 0.2f;
    }

    public override void JumpStart()
    {
        //check the player's distance from the ground
        //if there is ground within a certain distance, buffer a jump
        //otherwise perform an air dive
        if (pc.jumpsRemaining > 0)
        {
            sm.ChangeState(sm.stateJumping);
            return;
        }
        if (c_jumpBuffer != null)
        {
            CoroutineRunner.Instance.StopCoroutine(c_jumpBuffer);
            c_jumpBuffer = null;
        }
        c_jumpBuffer = CoroutineRunner.Instance.StartCoroutine(JumpBuffer());
    }

    public override void DiveStart()
    {
        if (pc.canDive == false) return;
        pc.canDive = false;
        sm.ChangeState(sm.stateAirDive);
    }

    public override void GroundedStart()
    {
        if (c_jumpBuffer != null)
        {
            pc.canJump = true;
            sm.ChangeState(sm.stateJumping);
            CoroutineRunner.Instance.StopCoroutine(c_jumpBuffer);
            c_jumpBuffer = null;
            return;
        }

        if (pc.crouchHeld)
        {
            if (pc.rb.linearVelocity.magnitude > 0.5f && pc.gatedDir != Vector2.zero)
            {
                sm.ChangeState(sm.stateSliding);
                return;
            }
            /*            else sm.ChangeState(sm.stateCrouching);*/
        }

        sm.ChangeState(sm.stateStanding);
    }

    public override void TailSwipeStart()
    {
        sm.ChangeState(sm.stateTailSwipe);
    }
}
