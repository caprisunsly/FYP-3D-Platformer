using System.Collections;
using UnityEngine;

public class State_Falling : Base_State
{
    [Header("Falling")]
    Coroutine c_jumpBuffer;
    [SerializeField] float jumpBufferTime;
    [SerializeField] Transform ledgeDetection;
    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;

    public override void StateEntry()
    {
        base.StateEntry();
        pc.modelAnim.SetTrigger("Fall");
    }

    IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        c_jumpBuffer = null;
    }

    private void OnDrawGizmos()
    {
        Vector3 heldDir = Vector3.zero;
        if (Application.isPlaying)
        {
            heldDir = pc.orientation.transform.forward * pc.dir.y + pc.orientation.transform.right * pc.dir.x;
        }
        else heldDir = transform.forward;

        Gizmos.DrawLine(ledgeDetection.position, ledgeDetection.position + heldDir * ledgeSnapDistance);
        Gizmos.DrawLine(ledgeDetection.position + new Vector3(0, ledgeHeight, 0), ledgeDetection.position + new Vector3(0, ledgeHeight, 0) + heldDir * ledgeSnapDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * minimumHeightFromGround);
    }
    

    public override void StateLogic()
    {
        base.StateLogic();
        //in front of the player refers to the direction they are moving
        Vector2 dirToLook = pc.FindVelRelativeToLook();
        Vector3 heldDir = new Vector3(dirToLook.x, 0, dirToLook.y);
        //if there is no ground in front of the player, there is no ledge to grab, so return
        if (!Physics.Raycast(ledgeDetection.position, heldDir, ledgeSnapDistance, pc.whatIsGround)) return;
        //if there is ground above the first ray, then we arent at the top of the wall, so return
        if (Physics.Raycast(ledgeDetection.position + new Vector3(0, ledgeHeight, 0), heldDir, ledgeSnapDistance, pc.whatIsGround)) return;
        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(transform.position, Vector3.down, minimumHeightFromGround, pc.whatIsGround)) return;

        sm.ChangeState(sm.stateLedgeHang);
    }


    public override void JumpStart()
    {
        if (c_jumpBuffer != null)
        {
            StopCoroutine(c_jumpBuffer);
            c_jumpBuffer = null;
        }
        c_jumpBuffer = StartCoroutine(JumpBuffer());
    }

    public override void GroundedStart()
    {
        if (c_jumpBuffer != null)
        {
            sm.ChangeState(sm.stateJumping);
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
