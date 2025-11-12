using System.Collections;
using UnityEngine;

public class State_LedgeHang : Base_State
{
    [Header("Falling")]
    [SerializeField] Transform ledgeDetection;
    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;
    [SerializeField] float yOffsetFromLedge;
    Vector3 point, direction, startPos;
    float time;
    Coroutine c_lerpToLedge;

    private void OnDrawGizmos()
    {
        Vector3 heldDir = Vector3.zero;
        if (Application.isPlaying)
        {
            heldDir = pc.orientation.transform.forward * pc.dir.y + pc.orientation.transform.right * pc.dir.x;
        }
        else heldDir = transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(ledgeDetection.position + heldDir * ledgeSnapDistance, ledgeDetection.position + new Vector3(0, ledgeHeight, 0) + heldDir * ledgeSnapDistance);
        Gizmos.DrawCube(point, Vector3.one * 0.1f);
    }

    public override void StateEntry()
    {
        base.StateEntry();
        pc.canJump = true;
        pc.modelAnim.SetBool("LedgeHang", true);
        time = 0;
        Vector3 heldDir = pc.orientation.transform.forward * pc.dir.y + pc.orientation.transform.right * pc.dir.x;
        //find the top of the ledge with a raycast sweep, using the collision point plus an offset to position the player at the top of the ledge
        Physics.Raycast(ledgeDetection.position, heldDir, out RaycastHit hitXZ, ledgeSnapDistance, pc.whatIsGround); 
        Physics.Raycast(ledgeDetection.position + new Vector3(0, ledgeHeight, 0) + heldDir * ledgeSnapDistance, Vector3.down * ledgeHeight, out RaycastHit hitY, ledgeSnapDistance, pc.whatIsGround);

         if (hitXZ.collider == null || hitY.collider == null)
         {
            sm.ChangeState(sm.stateFalling);
            return;
         }

        pc.jumpsRemaining = pc.totalJumps;

        direction = hitXZ.normal;

        point = new Vector3(pc.cl.radius * direction.x + hitXZ.point.x, yOffsetFromLedge + hitY.point.y, pc.cl.radius * direction.z + hitXZ.point.z);

        startPos = transform.position;

        c_lerpToLedge = StartCoroutine(C_LerpToLedge());
    }

    public override void StateExit()
    {
        base.StateEntry();
        pc.modelAnim.SetBool("LedgeHang", false);
        StopCoroutine(c_lerpToLedge);
        c_lerpToLedge = null;
    }

    IEnumerator C_LerpToLedge()
    {
        float time = 0;
        while (time < 1)
        {
            time += 0.34f;
            transform.position = Vector3.Lerp(startPos, point, time);
            pc.playerModel.rotation = Quaternion.Slerp(pc.playerModel.rotation, Quaternion.LookRotation(-direction, transform.up), time);
            for (int i = 0; i < 3; i++)
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
        pc.modelAnim.SetTrigger("Jump");
    }

    public override void CrouchStart()
    {
        sm.ChangeState(sm.stateFalling);
        pc.modelAnim.SetTrigger("Fall");
    }

}
