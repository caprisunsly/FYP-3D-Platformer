using System.Collections;
using UnityEngine;

public class State_Jumping : Base_State
{
    [Header("Jumping")]
    [SerializeField] float jumpForce = 550f;
    [SerializeField] float jumpCancelForce = 200f;

    bool jumping;
    [SerializeField] float jumpBufferTime = .2f;
    Coroutine c_jumpCancel, c_jumpBuffer;

    public override void StateEntry()
    {
        jumping = true;
        Jump();
    }

    IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        c_jumpBuffer = null;
    }

    private void Jump()
    {
        if (pc.grounded && pc.jumpsRemaining > 0)
        {
            pc.jumpsRemaining--;

            //Add jump forces
            pc.rb.AddForce(Vector2.up * jumpForce /* * .75 */, ForceMode.Impulse);
            //sends the player at the angle of the ground they are standing on
            /*            rb.AddForce(normalVector * jumpForce * 0.25f, ForceMode.Impulse);*/

            //If jumping while falling, reset y velocity. just in case i add a double jump
            Vector3 vel = pc.rb.linearVelocity;
            if (pc.rb.linearVelocity.y < 0.5f)
                pc.rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
            else if (pc.rb.linearVelocity.y > 0)
                pc.rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);
        }
    }

    public override void JumpStart()
    {
        jumping = true;
        if (c_jumpBuffer != null)
        {
            StopCoroutine(c_jumpBuffer);
            c_jumpBuffer = null;
        }
        c_jumpBuffer = StartCoroutine(JumpBuffer());
    }

    public override void JumpCancel()
    {
        jumping = false;
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
        if (c_jumpBuffer == null) sm.ChangeState(sm.stateStanding);
        else Jump();
    }
}
