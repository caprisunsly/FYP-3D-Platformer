using System.Collections;
using UnityEngine;

public class State_Crouching : Base_State
{
    [Header("Crouching")]
    Coroutine c_checkCrouchExit;

    float offset;

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        offset = pc.cl.center.y;
        pc.cl.height = pc.crouchingHeight;
        pc.cl.center = new Vector3(0, -pc.crouchingHeight/2 + offset, 0);
    }

    public override void StateExit()
    {
        base.StateExit();

        pc.cl.height = pc.standingHeight;
        pc.cl.center = new Vector3 (0, offset, 0);
    }

    public override void CrouchStart()
    {
        if (c_checkCrouchExit != null)
        {
            CoroutineRunner.Instance.StopCoroutine(c_checkCrouchExit);
            c_checkCrouchExit = null;
        }
    }

    public override void CrouchCancel()
    {
        if (c_checkCrouchExit != null)
        {
            CoroutineRunner.Instance.StopCoroutine(c_checkCrouchExit);
        }
        c_checkCrouchExit = CoroutineRunner.Instance.StartCoroutine(C_CheckCrouchExit());
    }

    IEnumerator C_CheckCrouchExit()
    {
        bool crouching = true;
        while (crouching)
        {
            yield return null;
            //check if there any objects where the player would stand
            if (Physics.Raycast(new Vector3(pc.transform.position.y, pc.transform.position.y - pc.cl.height / 2, pc.transform.position.z), pc.transform.up, pc.standingHeight)) continue;

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
