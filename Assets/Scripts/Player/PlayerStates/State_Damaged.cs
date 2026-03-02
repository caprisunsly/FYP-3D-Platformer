using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/Damaged")]
public class State_Damaged : Base_State
{
    Coroutine c_damaged;
    [SerializeField] float hitstunTime;
    [SerializeField] float knockbackForceH;
    [SerializeField] float knockbackForceV;


    bool diveAttempted = false;
    bool swipeAttempted = false;
    bool hitstun = false;

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        diveAttempted = false;
        swipeAttempted = false;
        base.StateEntry(PC, SM);
        c_damaged = CoroutineRunner.Instance.StartCoroutine(C_Damaged());
        pc.canDive = true;
        pc.jumpsRemaining = 0;
        pc.modelAnim.SetBool("Hurt", true);
    }

    public override void StateExit()
    {
        c_damaged = null;
    }

    IEnumerator C_Damaged()
    {
        pc.rb.linearVelocity = Vector3.zero;
        pc.rb.AddForce(pc.knockbackDir * knockbackForceH + Vector3.up * knockbackForceV, ForceMode.Impulse);
        pc.playerModel.transform.rotation = Quaternion.LookRotation(-pc.knockbackDir, Vector3.up);

        hitstun = true;
        yield return new WaitForSeconds(hitstunTime);
        hitstun = false;
        if (diveAttempted)
        {
            sm.ChangeState(sm.stateAirDive);
            pc.canDive = false;
        }
        else if (swipeAttempted) sm.ChangeState(sm.stateTailSwipe);
    }

    public override void DiveStart()
    {
        if (!hitstun)
        {
            sm.ChangeState(sm.stateAirDive);
            pc.modelAnim.SetBool("Hurt", false);
        }
        else
        {
            if (pc.canDive && !Physics.Raycast(pc.transform.position, Vector3.down, pc.minGroundDistance))
            {
                diveAttempted = true;
                swipeAttempted = false;
            }
        }
    }

    public override void TailSwipeStart()
    {
        if (!hitstun)
        {
            sm.ChangeState(sm.stateTailSwipe);
            pc.modelAnim.SetBool("Hurt", false);
        }
        else
        {
            swipeAttempted = true;
            diveAttempted = false;
        }
    }

    public override void GroundedStart()
    {
        if (pc.crouchHeld)
        {
            if (pc.rb.linearVelocity.magnitude > 0.5f && pc.gatedDir != Vector2.zero)
            {
                sm.ChangeState(sm.stateSliding);
                return;
            }
        }

        sm.ChangeState(sm.stateStanding);
    }

}
