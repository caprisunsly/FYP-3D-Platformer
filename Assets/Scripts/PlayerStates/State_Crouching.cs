using System.Collections;
using UnityEngine;

public class State_Crouching : Base_State
{
    [Header("Crouching")]
    Coroutine c_checkCrouchExit;

    public override void StateEntry()
    {
        base.StateEntry();
        pc.cl.height = pc.crouchingHeight;
        pc.cl.center = new Vector3(0, -pc.crouchingHeight/2, 0);
        pc.playerModel.localScale = new Vector3(1, pc.crouchingHeight/2, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, -pc.crouchingHeight / 2, 0);
    }

    public override void StateExit()
    {
        base.StateExit();

        pc.cl.height = pc.standingHeight;
        pc.cl.center = Vector3.zero;
        pc.playerModel.localScale = new Vector3(1, 1, 1);
        pc.playerModel.transform.localPosition = new Vector3(0, 0, 0);

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
        sm.ChangeState(sm.stateJumping);
    }

    public override void GroundedEnd()
    {
        sm.ChangeState(sm.stateFalling);
    }
}
