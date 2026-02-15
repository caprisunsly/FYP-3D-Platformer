using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/TailSwipe")]
public class State_TailSwipe : Base_State
{
    [SerializeField] float swipeTime = .4f;
    [SerializeField] float swipeDelay = .15f; //time before the hitbox comes out
    [SerializeField] float minimumHeightFromGround;
    [SerializeField] float minimumWallAngle;


    Coroutine c_swipe;

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        c_swipe = CoroutineRunner.Instance.StartCoroutine(C_Swipe());
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_swipe != null) CoroutineRunner.Instance.StopCoroutine(c_swipe);
    }

    IEnumerator C_Swipe()
    {
        if (!pc.grounded) pc.airSwipesRemaining--;

        pc.modelAnim.SetTrigger("TailSwipe");

        float time = 0;
        if (pc.rb.linearVelocity.y < 0) pc.rb.linearVelocity = new Vector3(pc.rb.linearVelocity.x, 0, pc.rb.linearVelocity.z);


        while (time < swipeDelay)
        {
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        time = 0;
        //enable box collider for dealing damage. i could tie the collider to the tail, but due to the lack of blending in animations it would be a very inconsistent hitbox.
        while (time < swipeTime)
        {
            if (pc.contactingWall)
            {
                if (pc.airSwipesRemaining >= 0)
                {
                    Debug.Log("enterbounce");
                    sm.ChangeState(sm.stateWallBounce);
                }
            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        //disable box collider for damage


        //check what state the player should enter, standing or falling
        if (pc.grounded) sm.ChangeState(sm.stateStanding);
        else sm.ChangeState(sm.stateFalling);
    }

    public override void DiveStart()
    {
        if (pc.canDive == false) return;
        pc.canDive = false;
        sm.ChangeState(sm.stateAirDive);
    }
}