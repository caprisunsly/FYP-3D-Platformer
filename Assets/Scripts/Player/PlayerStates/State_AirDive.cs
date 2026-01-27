using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/AirDive")]
public class State_AirDive : Base_State
{
    [Header("Dive")]
    [SerializeField] float diveForceH = 800f;
    [SerializeField] float diveForceV = 800f;
    [SerializeField] float diveTime = .4f;
    [SerializeField] float diveDelay = .15f;
    Vector2 dir;
    Coroutine c_diving;
    bool jumpAttempted, swipeAttempted;

    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;
    LedgeCastData ledgeData;


    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        ledgeData = new LedgeCastData(pc, ledgeHeight, ledgeSnapDistance, minimumHeightFromGround);
        c_diving = CoroutineRunner.Instance.StartCoroutine(C_Diving());
        pc.divesRemaining--;
        pc.jumpsRemaining = 1;
        pc.canJump = true;
        jumpAttempted = false;
        swipeAttempted = false;
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_diving != null) CoroutineRunner.Instance.StopCoroutine(c_diving);
    }

    IEnumerator C_Diving()
    {
        float time = 0;

        Vector3 oldvel = pc.rb.linearVelocity;
        pc.rb.linearVelocity = Vector3.zero;

        while (time < diveDelay) //gives the player a moment to react to the dive input
        {
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        time = 0;

        dir = pc.ungatedDir;
        if (dir == Vector2.zero) dir = new Vector2(0, 1); //prevents player from going nowhere on a dive. they will go in the camera forward if they arent inputting
        Vector3 movement = Vector3.ClampMagnitude(pc.orientation.transform.forward * dir.y + pc.orientation.transform.right * dir.x, 1);
        
        pc.rb.AddForce(movement * diveForceH + Vector3.up * diveForceV, ForceMode.Impulse);
        pc.modelAnim.SetTrigger("Dive");
        while (time < diveTime)
        {
            if (LedgeCast.Check(ledgeData, pc.ungatedDir))
            {
                sm.ChangeState(sm.stateLedgeHang);
            }
            time += Time.fixedDeltaTime;
            if (time > diveTime / 2)
            {
                if (pc.jumpsRemaining > 0 && jumpAttempted) sm.ChangeState(sm.stateJumping);
                if (swipeAttempted) sm.ChangeState(sm.stateTailSwipe);
            }
            yield return new WaitForFixedUpdate();
        }
        if (!jumpAttempted) sm.ChangeState(sm.stateFalling);
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
        jumpAttempted = true;
        swipeAttempted = false;
    }
    public override void TailSwipeStart()
    {
        swipeAttempted = true;
        jumpAttempted = false;
    }
}

public class LedgeCast : MonoBehaviour
{
    public static bool Check(LedgeCastData data, Vector2 direction)
    {
        //in front of the player refers to the direction they are moving
        Vector3 forwardRay = data.orientation.forward * direction.y + data.orientation.right * direction.x;

        if (direction.magnitude < 0.1f) return false;

        //if there is no ground in front of the player to the left or right (to fill edge cases) so return
        if (!Physics.Raycast(data.ledgeDetection.position, forwardRay, data.distance, data.whatIsGround)) 
            return false;

        //if there is ground a set amount above the first ray, then we arent at the top of the wall, so return
        if (Physics.Raycast(data.ledgeDetection.position + new Vector3(0, data.height, 0), forwardRay, data.distance + 1, data.whatIsGround)) return false;

        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(data.orientation.position, Vector3.down, data.groundHeight, data.whatIsGround) && data.orientation.position.y - data.startHeight < data.groundHeight) return false;

        return true;
    }
}

public struct LedgeCastData
{
    public LedgeCastData(PlayerController pc, float Height, float Distance, float GroundHeight)
    {
        orientation = pc.orientation;
        ledgeDetection = pc.ledgeDetection;
        whatIsGround = pc.whatIsGround;
        startHeight = pc.jumpedFrom;
        height = Height;
        distance = Distance;
        groundHeight = GroundHeight;
    }

    public Transform orientation;
    public Transform ledgeDetection;
    public LayerMask whatIsGround;
    public float startHeight;
    public float height;
    public float distance;
    public float groundHeight;
}
