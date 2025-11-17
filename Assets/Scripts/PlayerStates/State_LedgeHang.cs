using System.Collections;
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

    public override void StateEntry(PlayerController PC, PlayerStateManager SM)
    {
        base.StateEntry(PC, SM);
        pc.canJump = true;
        pc.canDive = true;
        pc.modelAnim.SetBool("LedgeHang", true);
        Vector3 heldDir = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * pc.ungatedDir.x;
        //find the top of the ledge with a raycast sweep, using the collision point plus an offset to position the player at the top of the ledge
        Physics.Raycast(pc.ledgeDetection.position, heldDir, out RaycastHit hitXZ, ledgeSnapDistance, pc.whatIsGround); 
        Physics.Raycast(pc.ledgeDetection.position + new Vector3(0, ledgeHeight, 0) + heldDir * ledgeSnapDistance, Vector3.down * ledgeHeight, out RaycastHit hitY, ledgeSnapDistance, pc.whatIsGround);

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
