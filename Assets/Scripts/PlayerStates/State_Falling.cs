using System.Collections;
using UnityEngine;

public class State_Falling : Base_State
{

    Coroutine c_jumpBuffer;
    float jumpBufferTime;
    [SerializeField] float airAccel = 300f;
    bool moving;
    Coroutine c_movement;

    public override void StateEntry()
    {
        Movement(pc.dir);
    }

    public override void StateExit()
    {
        if (c_movement != null)
        {
            StopCoroutine(c_movement);
            c_movement = null;
        }
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

    public override void GroundedStart()
    {
        if (c_jumpBuffer != null)
        {
            sm.ChangeState(sm.stateJumping);
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
