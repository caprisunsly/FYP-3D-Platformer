using System.Collections;
using UnityEngine;

public class State_Standing : Base_State
{
    Coroutine c_movement;
    bool moving = true;

    //Some multipliers for other scripts to use
    float multiplier = 1f;

    public override void StateEntry()
    {
        moving = true;
    }

    public override void StateExit()
    {
        if (c_movement != null) StopCoroutine(c_movement);
        c_movement = null;
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

/*            //slows the player down if they arent inputting anything
            CounterMovement(dir.x, dir.y, mag);*/

            //If speed is larger than maxspeed, cancel out the input so player doesn't go over max speed
            Vector2 appliedDir = dir;

            if (dir.x > 0 && mag.x > pc.moveSpeedMax) appliedDir.x = 0;
            if (dir.x < 0 && mag.x < -pc.moveSpeedMax) appliedDir.x = 0;
            if (dir.y > 0 && mag.y > pc.moveSpeedMax) appliedDir.y = 0;
            if (dir.y < 0 && mag.y < -pc.moveSpeedMax) appliedDir.y = 0;

            //Apply forces to move player
            pc.rb.AddForce(pc.orientation.transform.forward * appliedDir.y * pc.moveSpeedAccel * multiplier);
            pc.rb.AddForce(pc.orientation.transform.right * appliedDir.x * pc.moveSpeedAccel * multiplier);

            if (dir == Vector2.zero && pc.rb.linearVelocity.magnitude == 0) moving = false;
            yield return new WaitForFixedUpdate();
        }
        c_movement = null;
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!pc.grounded) return;

        //Counter movement
        if (Mathf.Abs(mag.x) > 0.01f && Mathf.Abs(x) < 0.05f || (mag.x < -0.01f && x > 0) || (mag.x > 0.01f && x < 0))
        {
            pc.rb.AddForce(pc.moveSpeedAccel * pc.orientation.transform.right * -mag.x * .175f);
        }
        if (Mathf.Abs(mag.y) > 0.01f && Mathf.Abs(y) < 0.05f || (mag.y < -0.01f && y > 0) || (mag.y > 0.01f && y < 0))
        {
            pc.rb.AddForce(pc.moveSpeedAccel * pc.orientation.transform.forward * -mag.y * .175f);
        }
    }

    public override void JumpStart()
    {
        sm.ChangeState(sm.stateJumping);
    }

    public override void CrouchStart()
    {
        if (pc.rb.linearVelocity.magnitude > 0.5f) sm.ChangeState(sm.stateSliding);
        else sm.ChangeState(sm.stateCrouching);
    }
}
