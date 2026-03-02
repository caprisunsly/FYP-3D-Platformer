using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/LedgeHang")]
public class State_LedgeHang : Base_State
{
    [Header("Falling")]
    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float yOffsetFromLedge;
    [SerializeField] float jumpForce;
    Vector3 point, direction, startPos;
    [SerializeField] float numberOfFrames, framerate;
    Coroutine c_lerpToLedge;
    RaycastHit foundWall;


    private void OnEnable()
    {
        LedgeCast.OnFoundLedge += SetWall;
    }

    private void OnDisable()
    {
        LedgeCast.OnFoundLedge -= SetWall;
    }

    void SetWall(RaycastHit wall)
    {
        foundWall = wall;
    }

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        pc.canJump = true;
        pc.canDive = true;
        pc.modelAnim.SetBool("LedgeHang", true);
        Vector3 heldDir = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * pc.ungatedDir.x;
        //find the top of the ledge with a raycast sweep, using the collision point plus an offset to position the player at the top of the ledge
        Physics.Raycast(pc.ledgeDetection.position, foundWall.point - pc.ledgeDetection.position, out RaycastHit hitXZ, ledgeSnapDistance * 2, pc.whatIsWall); 
        Physics.Raycast(foundWall.point + new Vector3(0, ledgeHeight, 0) + (foundWall.point - pc.ledgeDetection.position), Vector3.down, out RaycastHit hitY, ledgeHeight, pc.whatIsWall);

         if (hitXZ.collider == null || hitY.collider == null)
         {
            sm.ChangeState(sm.stateFalling);
            return;
         }

        pc.jumpsRemaining = pc.totalJumps;

        direction = hitXZ.normal;

        point = new Vector3(pc.cl.radius * direction.x + hitXZ.point.x, yOffsetFromLedge + hitY.point.y, pc.cl.radius * direction.z + hitXZ.point.z);

        startPos = pc.transform.position;

        c_lerpToLedge = CoroutineRunner.Instance.StartCoroutine(C_LerpToLedge());
    }

    public override void StateExit()
    {
        base.StateExit();
        pc.modelAnim.SetBool("LedgeHang", false);
        if (c_lerpToLedge != null) CoroutineRunner.Instance.StopCoroutine(c_lerpToLedge);
        c_lerpToLedge = null;
    }

    IEnumerator C_LerpToLedge()
    {
        float time = 0;
        while (time < 1)
        {
            time += 1 / numberOfFrames;
            pc.transform.position = Vector3.Lerp(startPos, point, time);
            pc.playerModel.rotation = Quaternion.Slerp(pc.playerModel.rotation, Quaternion.LookRotation(-direction, pc.transform.up), time);
            for (int i = 0; i < 50/framerate; i++)
            {
                yield return new WaitForFixedUpdate();
                i++;
            }
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
        pc.rb.linearVelocity = Vector3.zero;
    }

    public override void JumpStart()
    {
        sm.ChangeState(sm.stateJumping);
        pc.rb.AddForce(Vector3.ClampMagnitude(pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * pc.ungatedDir.x, 1) * jumpForce, ForceMode.Impulse);
        pc.modelAnim.SetTrigger("Jump");
    }

    public override void CrouchStart()
    {
        sm.ChangeState(sm.stateFalling);
        pc.modelAnim.SetTrigger("Fall");
    }

}


public class LedgeCast : MonoBehaviour
{
    public static event Action<RaycastHit> OnFoundLedge;

    public static bool Check(LedgeCastData data, Vector2 direction)
    {
        //in front of the player refers to the direction they are moving
        /*        Vector3 forwardRay = data.orientation.forward * direction.y + data.orientation.right * direction.x;
        */

        Vector3 forwardRay = (data.orientation.forward * direction.y + data.orientation.right * direction.x).normalized;

        if (direction.magnitude < 0.1f) return false;


        if(!Physics.BoxCast(data.ledgeDetection.position, new Vector3(data.distance/2, .25f, .01f), forwardRay, out RaycastHit info, 
            Quaternion.LookRotation(forwardRay, Vector3.up), data.distance, data.whatIsGround, QueryTriggerInteraction.Ignore))
            return false;
        Debug.Log("Found Surface");

        /*        //if there is no ground in front of the player so return. also ignores trigger colliders.
                if (!Physics.Raycast(data.ledgeDetection.position, forwardRay, data.distance, data.whatIsGround, QueryTriggerInteraction.Ignore))
                    return false;*/

        //if there is ground a set amount above the first ray, then we arent at the top of the wall, so return
        if (Physics.CheckBox(data.ledgeDetection.position + forwardRay + new Vector3(0, data.height, 0), new Vector3(data.distance, .25f, data.distance), Quaternion.LookRotation(forwardRay, Vector3.up),
            data.whatIsGround, QueryTriggerInteraction.Ignore)) return false;

        Debug.Log("Found Edge");

        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(data.orientation.position - forwardRay/2, Vector3.down, data.groundHeight, data.whatIsGround, QueryTriggerInteraction.Ignore)) return false;

        Debug.Log("Found Ground");


        OnFoundLedge?.Invoke(info);
         return true;
    }
}

public struct LedgeCastData
{
    public LedgeCastData(PlayerController pc, float Height, float Distance, float GroundHeight)
    {
        orientation = pc.orientation;
        ledgeDetection = pc.ledgeDetection;
        whatIsGround = pc.whatIsWall;
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
