using System.Collections;
using UnityEngine;

public class State_Jumping : Base_State
{
    [Header("Jumping")]
    [SerializeField] float jumpForce = 550f;
    [SerializeField] float jumpCancelForce = 200f;

    [SerializeField] float jumpBufferTime = .2f;
    Coroutine c_jumpCancel, c_jumpBuffer;
    bool fall;
    int startFrames;

    [SerializeField] Transform ledgeDetection;
    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;

    public override void StateEntry()
    {
        base.StateEntry();
        fall = false;
        startFrames = 5;
        pc.modelAnim.SetBool("Standing", false);
        StartCoroutine(pc.C_OverrideSlopeDirection());
        Jump();
        if (!pc.jumpHeld) Invoke(nameof(JumpCancel), 0.1f); //prevents buffered jumps from the falling state from always being max height
    }

    IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        c_jumpBuffer = null;
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

    public override void StateFixedUpdate()
    {
        if (startFrames > 0) //prevents the player entering the fall state immediately after jumping
        {
            startFrames--;
            return;
        }
        CheckWallHang();
        if (pc.rb.linearVelocity.y > 0) return;
        if (fall) return;
        fall = true;

        sm.ChangeState(sm.stateFalling);
    }

    void CheckWallHang()
    {
        if (pc.rb.linearVelocity.y < 3) return;
        //in front of the player refers to the direction they are moving
        Vector3 heldDir = pc.orientation.transform.forward * pc.dir.y + pc.orientation.transform.right * pc.dir.x;

        //if there is no ground in front of the player, there is no ledge to grab, so return
        if (!Physics.Raycast(ledgeDetection.position, heldDir, ledgeSnapDistance, pc.whatIsGround)) return;
        //if there is ground a set amount above the first ray, then we arent at the top of the wall, so return
        if (Physics.Raycast(ledgeDetection.position + new Vector3(0, ledgeHeight, 0), heldDir, ledgeSnapDistance, pc.whatIsGround)) return;
        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(transform.position, Vector3.down, minimumHeightFromGround, pc.whatIsGround)) return;

        sm.ChangeState(sm.stateLedgeHang);
    }

    private void Jump()
    {
        if (pc.canJump && pc.jumpsRemaining > 0)
        {
            pc.canJump = false;
            pc.jumpsRemaining--;
            pc.modelAnim.SetTrigger("Jump");

            //Add jump forces
            pc.rb.AddForce(Vector2.up * jumpForce /* * .75 */, ForceMode.Impulse);
            //sends the player at the angle of the ground they are standing on
            /*            rb.AddForce(normalVector * jumpForce * 0.25f, ForceMode.Impulse);*/

            //If jumping while falling, reset y velocity. just in case i add a double jump
            Vector3 vel = pc.rb.linearVelocity;
            if (pc.rb.linearVelocity.y < 0.5f) pc.rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
            else if (pc.rb.linearVelocity.y > 0) pc.rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);

/*            //prevent player from moving a tiny bit in air
            if (vel.x < .5f) pc.rb.linearVelocity = new Vector3(0, vel.y, vel.z);
            if (vel.z < .5f) pc.rb.linearVelocity = new Vector3(vel.x, vel.y, 0);*/
        }
    }

    public override void JumpCancel()
    {
        if (pc.jumpHeld) return;
        if (c_jumpCancel != null)
        {
            StopCoroutine(c_jumpCancel);
            c_jumpCancel = null;
        }
        c_jumpCancel = StartCoroutine(C_JumpCancel());
    }

    public IEnumerator C_JumpCancel()
    {
        while (pc.rb.linearVelocity.y > 0)
        {
            pc.rb.AddForce(new Vector3(0, -pc.rb.linearVelocity.y, 0) * jumpCancelForce);
            yield return new WaitForFixedUpdate();
        }
        c_jumpCancel = null;
    }

    public override void GroundedStart()
    {
        if (c_jumpBuffer != null)
        {
            Jump();
            return;
        }

        if (pc.crouchHeld) 
        {
            if (pc.rb.linearVelocity.magnitude > 0.5f && pc.dir != Vector2.zero)
            {
                sm.ChangeState(sm.stateSliding);
                return;
            }
        }

        sm.ChangeState(sm.stateStanding);
    }
}
