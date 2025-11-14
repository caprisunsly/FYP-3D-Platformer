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

    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float minimumHeightFromGround;


    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        c_diving = CoroutineRunner.Instance.StartCoroutine(C_Diving());
    }

    public override void StateExit()
    {
        base.StateExit();
        if (c_diving != null) CoroutineRunner.Instance.StopCoroutine(c_diving);
    }

    IEnumerator C_Diving()
    {
        float time = 0;

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
            CheckWallHang();
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        sm.ChangeState(sm.stateFalling);
    }

    void CheckWallHang()
    {
        if (pc.rb.linearVelocity.y < 3) return;
        //in front of the player refers to the direction they are moving
        Vector3 leftRay = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * (pc.ungatedDir.x + 1);
        Vector3 rightRay = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * (pc.ungatedDir.x - 1);

        //if there is no ground in front of the player, there is no ledge to grab, so return
        if (!Physics.Raycast(pc.ledgeDetection.position, leftRay, ledgeSnapDistance, pc.whatIsGround) && !Physics.Raycast(pc.ledgeDetection.position, rightRay, ledgeSnapDistance, pc.whatIsGround)) return;
        //if there is ground a set amount above the first ray, then we arent at the top of the wall, so return
        if (Physics.Raycast(pc.ledgeDetection.position + new Vector3(0, ledgeHeight, 0), leftRay, ledgeSnapDistance, pc.whatIsGround)) return;
        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(pc.transform.position, Vector3.down, minimumHeightFromGround, pc.whatIsGround) && pc.transform.position.y - pc.jumpedFrom < minimumHeightFromGround) return;

        sm.ChangeState(sm.stateLedgeHang);
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
