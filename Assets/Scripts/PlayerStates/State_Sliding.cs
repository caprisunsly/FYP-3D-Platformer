using System.Collections;
using UnityEngine;

public class State_Sliding : Base_State
{
    [SerializeField] float slideAccel = 400;
    [SerializeField] float slideMaxSpeed = 400;
    [SerializeField] float slideTime = 400;
    [SerializeField] float slideCounterMovement = 0.2f;

    bool crouching = true;

    public override void StateEntry()
    {
        base.StateEntry();
        pc.cl.height = pc.crouchingHeight;
        pc.cl.center = new Vector3(0, -pc.crouchingHeight / 2, 0);
        pc.playerModel.localScale = new Vector3(1, pc.crouchingHeight/2, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, -pc.crouchingHeight / 2, 0);
        StartCoroutine(C_Sliding());
    }

    public override void StateExit()
    {
        base.StateExit();
        pc.cl.height = pc.standingHeight;
        pc.cl.center = Vector3.zero;
        pc.playerModel.localScale = new Vector3(1, 1, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, 0, 0);
    }

    IEnumerator C_Sliding()
    {
        float time = 0;
        Vector3 movement = Vector3.ClampMagnitude(pc.orientation.transform.forward * pc.dir.y + pc.orientation.transform.right * pc.dir.x, 1);
        while (time < slideTime)
        {
            //if (!pc.grounded) sm.ChangeState(sm.stateFalling);
            //only apply the force if we're still below the max speed threshold
            if (pc.rb.linearVelocity.magnitude < slideMaxSpeed)
            {
                //Apply forces to move player
                pc.rb.AddForce(movement * slideAccel);
            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (crouching) sm.ChangeState(sm.stateCrouching);
        else sm.ChangeState(sm.stateStanding);
    }

    public override void JumpStart()
    {
        sm.ChangeState(sm.stateJumping);
    }

    public override void CrouchCancel()
    {
        crouching = false;
    }

    public override void CrouchStart()
    {
        crouching = true;
    }

    public override void GroundedEnd()
    {
        sm.ChangeState(sm.stateFalling);
    }
}
