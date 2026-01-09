using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/TailSwipe")]
public class State_TailSwipe : Base_State
{
    [SerializeField] float swipeTime = .4f;
    [SerializeField] float swipeDelay = .15f; //time before the hitbox comes out
    [SerializeField] float boxSizeY;
    [SerializeField] float boxSizeXZ;
    [SerializeField] float minimumHeightFromGround;
    [SerializeField] float minimumWallAngle;


    Coroutine c_swipe;
    LedgeCastData ledgeData;

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        ledgeData = new LedgeCastData(pc, boxSizeY, boxSizeXZ, minimumHeightFromGround);
        c_swipe = CoroutineRunner.Instance.StartCoroutine(C_Swipe());
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_swipe != null) CoroutineRunner.Instance.StopCoroutine(c_swipe);
    }

    IEnumerator C_Swipe()
    {
        pc.modelAnim.SetTrigger("TailSwipe");

        float time = 0;

        while (time < swipeDelay)
        {
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        time = 0;
        Debug.Log("start");
        //enable box collider for dealing damage. i could tie the collider to the tail, but due to the lack of blending in animations it would be a very inconsistent hitbox.
        while (time < swipeTime)
        {
            //anything that needs to be done during the swipe goes here.
            //check box around player
            RaycastHit hit = LedgeCast.CheckBox(ledgeData, pc.ungatedDir, minimumWallAngle);
            if (hit.rigidbody != null)
            {
                Debug.Log("found");
                //if wall -> enter bounce
                Vector3 direction = Vector3.Reflect(pc.transform.position - hit.point, hit.normal);
                Debug.DrawLine(direction, direction * 2, Color.red, 5);
            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        //disable box collider for damage


        //check what state the player should enter, standing or falling
        if (pc.grounded) sm.ChangeState(sm.stateStanding);
        else sm.ChangeState(sm.stateFalling);
    }
}