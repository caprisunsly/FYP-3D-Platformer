using System.Collections;
using UnityEngine;

public class State_Jumping : Base_State
{
    [Header("Jumping")]
    [SerializeField] float jumpForce = 550f;
    [SerializeField] float jumpCancelForce = 200f;
    [SerializeField] float airAccel = 300f;

    bool moving;
    [SerializeField] float jumpBufferTime = .2f;
    Coroutine c_jumpCancel, c_jumpBuffer, c_movement;
    bool fall;

    public override void StateEntry()
    {
        fall = false;
        Jump();
    }

    public override void Movement(Vector2 dir)
    {
        if (c_movement != null)
        {
            StopCoroutine(c_movement);
            c_movement = null;
        }
        moving = true;
        c_movement = StartCoroutine(C_Movement(dir));
    }

    public override void StateLogic()
    {
        if (pc.rb.linearVelocity.y > -1) return;
        if (fall) return;
        fall = true;

        sm.ChangeState(sm.stateFalling);
    }

    private IEnumerator C_Movement(Vector2 dir)
    {
        while (moving)
        {
            //Find actual velocity relative to where camera is looking
            Vector2 mag = pc.FindVelRelativeToLook();

            //slows the player down if they arent inputting anything
            /*            CounterMovement(dir.x, dir.y, mag);
            */
            //If speed is larger than maxspeed, cancel out the input so player doesn't go over max speed
            Vector2 appliedDir = dir;

            if (dir.x > 0 && mag.x > pc.moveSpeedMax) appliedDir.x = 0;
            if (dir.x < 0 && mag.x < -pc.moveSpeedMax) appliedDir.x = 0;
            if (dir.y > 0 && mag.y > pc.moveSpeedMax) appliedDir.y = 0;
            if (dir.y < 0 && mag.y < -pc.moveSpeedMax) appliedDir.y = 0;

            //Apply forces to move player
            Vector3 movement = Vector3.ClampMagnitude(pc.orientation.transform.forward * appliedDir.y + pc.orientation.transform.right * appliedDir.x, 1);

            //Apply forces to move player
            pc.rb.AddForce(movement * airAccel);

            if (dir == Vector2.zero && pc.rb.linearVelocity.magnitude == 0) moving = false;
            yield return new WaitForFixedUpdate();
        }
        c_movement = null;
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
            if (pc.rb.linearVelocity.y < 0.5f) pc.rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
            else if (pc.rb.linearVelocity.y > 0) pc.rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);

/*            //prevent player from moving a tiny bit in air
            if (vel.x < .5f) pc.rb.linearVelocity = new Vector3(0, vel.y, vel.z);
            if (vel.z < .5f) pc.rb.linearVelocity = new Vector3(vel.x, vel.y, 0);*/
        }
    }

    public override void JumpCancel()
    {
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
            if (pc.rb.linearVelocity.magnitude > 0.5f) sm.ChangeState(sm.stateSliding);
            else sm.ChangeState(sm.stateCrouching);
            return;
        }

        sm.ChangeState(sm.stateStanding);
    }
}
