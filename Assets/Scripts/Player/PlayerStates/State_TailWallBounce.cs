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
    Vector2 dir;
    Coroutine c_swipe;

    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;
    LedgeCastData ledgeData;
    Vector3 direction;


    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        c_swipe = CoroutineRunner.Instance.StartCoroutine(C_Diving());
        direction = Vector3.Reflect(pc.p.point - pc.transform.position, pc.p.normal).normalized;
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_swipe != null) CoroutineRunner.Instance.StopCoroutine(c_swipe);
    }

    IEnumerator C_Diving()
    {
        float time = 0;
        pc.rb.linearVelocity = new Vector3(pc.rb.linearVelocity.x, 0, pc.rb.linearVelocity.z);

        pc.modelAnim.SetTrigger("Bounce");

        pc.rb.AddForce(direction * diveForceH + Vector3.up * diveForceV, ForceMode.Impulse);
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
}