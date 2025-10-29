using System.Collections;
using UnityEngine;

public class State_Crouching : Base_State
{
    Coroutine c_movement;
    bool moving = true;
    [SerializeField] float crouchAccel;
    [SerializeField] float crouchMaxSpeed;
    Coroutine c_checkCrouchExit;

    //Some multipliers for other scripts to use
    float multiplier = 1f;

    public override void StateEntry()
    {
        base.StateEntry();
        pc.cl.height = pc.crouchingHeight;
        pc.cl.center = new Vector3(0, -pc.crouchingHeight/2, 0);
        pc.playerModel.localScale = new Vector3(1, pc.crouchingHeight/2, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, -pc.crouchingHeight / 2, 0);
        moving = true;
    }

    public override void StateExit()
    {
        base.StateExit();

        pc.cl.height = pc.standingHeight;
        pc.cl.center = Vector3.zero;
        pc.playerModel.localScale = new Vector3(1, 1, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, 0, 0);
        StopCoroutine(c_movement);
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

            //slows the player down if they arent inputting anything
/*            CounterMovement(dir.x, dir.y, mag);
*/
            //If speed is larger than maxspeed, cancel out the input so player doesn't go over max speed
            Vector2 appliedDir = dir;

            if (dir.x > 0 && mag.x > crouchMaxSpeed) appliedDir.x = 0;
            if (dir.x < 0 && mag.x < -crouchMaxSpeed) appliedDir.x = 0;
            if (dir.y > 0 && mag.y > crouchMaxSpeed) appliedDir.y = 0;
            if (dir.y < 0 && mag.y < -crouchMaxSpeed) appliedDir.y = 0;

            //Apply forces to move player
            pc.rb.AddForce(pc.orientation.transform.forward * appliedDir.y * crouchAccel * multiplier);
            pc.rb.AddForce(pc.orientation.transform.right * appliedDir.x * crouchAccel * multiplier);

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
            pc.rb.AddForce(crouchAccel * pc.orientation.transform.right * -mag.x * .175f);
        }
        if (Mathf.Abs(mag.y) > 0.01f && Mathf.Abs(y) < 0.05f || (mag.y < -0.01f && y > 0) || (mag.y > 0.01f && y < 0))
        {
            pc.rb.AddForce(crouchAccel * pc.orientation.transform.forward * -mag.y * .175f);
        }
    }

    public override void CrouchStart()
    {
        if (c_checkCrouchExit != null)
        {
            StopCoroutine(c_checkCrouchExit);
            c_checkCrouchExit = null;
        }
    }

    public override void CrouchCancel()
    {
        if (c_checkCrouchExit != null)
        {
            StopCoroutine(c_checkCrouchExit);
        }
        c_checkCrouchExit = StartCoroutine(C_CheckCrouchExit());
    }

    IEnumerator C_CheckCrouchExit()
    {
        bool crouching = true;
        while (crouching)
        {
            yield return null;
            //check if there any objects where the player would stand
            if (Physics.Raycast(new Vector3(transform.position.y, transform.position.y - pc.cl.height / 2, transform.position.z), transform.up, pc.standingHeight)) continue;

            crouching = false;
            sm.ChangeState(sm.stateStanding);
        }
    }

    public override void JumpStart()
    {
/*        sm.ChangeState(sm.stateCrouchJump);
*/    }

}
