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
        while (time < slideTime)
        {
            //if (!pc.grounded) sm.ChangeState(sm.stateFalling);
            //only apply the force if we're still below the max speed threshold
            if (pc.rb.linearVelocity.magnitude < slideMaxSpeed)
            {
                pc.rb.AddForce(pc.orientation.transform.forward * slideAccel);
            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (crouching) sm.ChangeState(sm.stateCrouching);
        else sm.ChangeState(sm.stateStanding);
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
