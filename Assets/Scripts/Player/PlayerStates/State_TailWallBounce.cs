using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/AirDive")]
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


    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        c_swipe = CoroutineRunner.Instance.StartCoroutine(C_Diving());
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_swipe != null) CoroutineRunner.Instance.StopCoroutine(c_swipe);
    }

    IEnumerator C_Diving()
    {
        float time = 0;

        while (time < diveDelay) //gives the player a moment to react to the dive input
        {
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        time = 0;

        pc.modelAnim.SetTrigger("Dive");
        while (time < diveTime)
        {

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