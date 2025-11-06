using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class State_Sliding : Base_State
{
    [Header("Sliding")]
    [SerializeField] float slideAccel = 400;
    [SerializeField] float slideMaxSpeed = 400;
    [SerializeField] float slideTime = 400;
    Vector2 dir;

    Coroutine c_sliding;

    public override void StateEntry()
    {
        base.StateEntry();
        dir = pc.dir; // store the direction the player is currently trying to move in
        pc.cl.height = pc.crouchingHeight;
        pc.cl.center = new Vector3(0, -pc.crouchingHeight / 2, 0);
        pc.playerModel.localScale = new Vector3(1, pc.crouchingHeight/2, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, -pc.crouchingHeight / 2, 0);
        c_sliding = StartCoroutine(C_Sliding());
    }

    public override void StateExit()
    {
        base.StateExit();
        pc.cl.height = pc.standingHeight;
        pc.cl.center = Vector3.zero;
        pc.playerModel.localScale = new Vector3(1, 1, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, 0, 0);
        StopCoroutine(c_sliding);
    }

    IEnumerator C_Sliding()
    {
        float time = 0;
        if (dir == Vector2.zero) dir = new Vector2(pc.rb.linearVelocity.x, pc.rb.linearVelocity.z).normalized; //prevents super slow slides when the player slides as they release movement keys
        Vector3 movement = Vector3.ClampMagnitude(pc.orientation.transform.forward * dir.y + pc.orientation.transform.right * dir.x, 1);

        while (time < slideTime)
        {
            //if (!pc.grounded) sm.ChangeState(sm.stateFalling);
            //only apply the force if we're still below the max speed threshold
            if (pc.rb.linearVelocity.magnitude < slideMaxSpeed)
            {
                //Apply forces to move player
                pc.rb.AddForce(Vector3.ProjectOnPlane(movement, pc.slopeDirection).normalized * slideAccel);

            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (pc.crouchHeld) sm.ChangeState(sm.stateCrouching);
        else sm.ChangeState(sm.stateStanding);
    }

    public override void JumpStart()
    {
        sm.ChangeState(sm.stateJumping);
    }

    public override void GroundedEnd()
    {
        sm.ChangeState(sm.stateFalling);
    }
}
