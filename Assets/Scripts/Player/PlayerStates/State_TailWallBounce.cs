using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/WallBounce")]
public class State_TailWallBounce : Base_State
{
    [Header("Dive")]
    [SerializeField] float diveForceH = 800f;
    [SerializeField] float diveForceV = 800f;
    [SerializeField] float diveTime = .4f;
    [SerializeField] float diveDelay = .15f;
    Coroutine c_swipe;

    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;
    [SerializeField] GameObject PS_Sparks;


    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        c_swipe = CoroutineRunner.Instance.StartCoroutine(C_Bouncing());
        /*        direction = Vector3.Reflect(pc.p.point - new Vector3(pc.transform.position.x, pc.p.point.y, pc.transform.position.z), pc.p.normal).normalized;
        */
        Instantiate(PS_Sparks, pc.p.point, Quaternion.LookRotation(pc.p.normal, Vector3.up));
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_swipe != null) CoroutineRunner.Instance.StopCoroutine(c_swipe);
    }

    IEnumerator C_Bouncing()
    {
        float time = 0;
        pc.rb.linearVelocity = new Vector3(pc.rb.linearVelocity.x, 0, pc.rb.linearVelocity.z);
        yield return new WaitForSeconds(0.02f);
        pc.modelAnim.SetTrigger("Bounce");

        Vector3 dir = Vector3.Reflect(pc.rb.linearVelocity.normalized - pc.p.normal, pc.p.normal).normalized;
        pc.rb.linearVelocity = diveForceH * dir + Vector3.up * diveForceV;

/*        pc.rb.AddForce(direction * diveForceH + Vector3.up * diveForceV, ForceMode.Impulse);
*/        Debug.DrawRay(pc.transform.position, pc.rb.linearVelocity, Color.blue, 2f);
        pc.canDive = true;
        pc.divesRemaining = 1;

        while (time < diveTime)
        {
            time += Time.deltaTime;
            yield return null;
        }
        sm.ChangeState(sm.stateFalling);
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

    public override void JumpStart()
    {
        if (pc.canDive == false) return;
        pc.canDive = false;
        sm.ChangeState(sm.stateAirDive);
    }
}