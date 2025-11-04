using System.Collections;
using UnityEngine;

public class State_Sliding : Base_State
{
    [SerializeField] float slideAccel = 400;
    [SerializeField] float slideMaxSpeed = 400;
    [SerializeField] float slideTime = 400;
    [SerializeField] float slideCounterMovement = 0.2f;
    Vector3 slideDir;

    bool crouching = true;

    public override void StateEntry()
    {
        base.StateEntry();
        slideDir = new Vector3(pc.dir.x, 0, pc.dir.y);
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

    public override void Movement(Vector2 dir)
    {
        slideDir = new Vector3(dir.x, 0, dir.y);
    }

    IEnumerator C_Sliding()
    {
        float time = 0;
        while (time < slideTime)
        {
            //if (!pc.grounded) sm.ChangeState(sm.stateFalling);
            //only apply the force if we're still below the max speed threshold
            if (pc.rb.linearVelocity.magnitude < slideMaxSpeed)
            {
                pc.rb.AddForce(slideDir * slideAccel);
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
}
