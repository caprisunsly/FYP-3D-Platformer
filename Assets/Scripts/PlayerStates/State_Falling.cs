using System.Collections;
using UnityEngine;

public class State_Falling : Base_State
{
    [Header("Falling")]
    Coroutine c_jumpBuffer;
    [SerializeField] float jumpBufferTime;

    public override void StateEntry()
    {
        base.StateEntry();
        pc.modelAnim.SetTrigger("Fall");
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

    public override void GroundedStart()
    {
        if (c_jumpBuffer != null)
        {
            sm.ChangeState(sm.stateJumping);
            return;
        }

        if (pc.crouchHeld)
        {
            if (pc.rb.linearVelocity.magnitude > 0.5f && pc.dir != Vector2.zero)
            {
                sm.ChangeState(sm.stateSliding);
                return;
            }
            /*            else sm.ChangeState(sm.stateCrouching);*/
        }

        sm.ChangeState(sm.stateStanding);
    }

}
