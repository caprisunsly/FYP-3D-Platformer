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

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        pc.modelAnim.SetBool("Fall", true);
    }

    public override void StateExit()
    {
        base.StateExit();
        pc.modelAnim.SetBool("Fall", false);
    }

    IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
         c_jumpBuffer = null;
    }

    public override void StateFixedUpdate()
    {
        if (pc.grounded) sm.ChangeState(sm.stateStanding); //prevents edge case where players jump but get stuck on geometry and don't leave the ground

        //in front of the player refers to the direction they are moving
        Vector3 heldDir = pc.orientation.transform.forward * pc.dir.y + pc.orientation.transform.right * pc.dir.x;

        //if there is no ground in front of the player, there is no ledge to grab, so return
        if (!Physics.Raycast(pc.ledgeDetection.position, heldDir, ledgeSnapDistance, pc.whatIsGround)) return;
        //if there is ground a set amount above the first ray, then we arent at the top of the wall, so return
        if (Physics.Raycast(pc.ledgeDetection.position + new Vector3(0, ledgeHeight, 0), heldDir, ledgeSnapDistance, pc.whatIsGround)) return;
        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(pc.transform.position, Vector3.down, minimumHeightFromGround, pc.whatIsGround)) return;

        sm.ChangeState(sm.stateLedgeHang);
    }


    public override void JumpStart()
    {
        if (c_jumpBuffer != null)
        {
            CoroutineRunner.Instance.StopCoroutine(c_jumpBuffer);
            c_jumpBuffer = null;
        }
        c_jumpBuffer = CoroutineRunner.Instance.StartCoroutine(JumpBuffer());
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
            if (pc.rb.linearVelocity.magnitude > 0.5f && pc.dir != Vector2.zero)
            {
                sm.ChangeState(sm.stateSliding);
                return;
            }
            /*            else sm.ChangeState(sm.stateCrouching);*/
        }

        sm.ChangeState(sm.stateStanding);
    }

}
