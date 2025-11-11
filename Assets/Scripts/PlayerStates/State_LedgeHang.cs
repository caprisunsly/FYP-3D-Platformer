using UnityEngine;

public class State_LedgeHang : Base_State
{
    [Header("Falling")]
    [SerializeField] Transform ledgeDetection;
    [SerializeField] float ledgeHeight;
    [SerializeField] float ledgeSnapDistance;

    public override void StateEntry()
    {
        base.StateEntry();
        pc.modelAnim.SetTrigger("LedgeHang");
        //find the top of the ledge with a raycast sweep, using the collision point plus an offset to position the player at the top of the ledge
        Physics.BoxCast(ledgeDetection.position + new Vector3(0, ledgeHeight, 0) + transform.forward * ledgeSnapDistance, Vector3.one, Vector3.down, out RaycastHit hit,
            Quaternion.identity, ledgeHeight, pc.whatIsGround);

        transform.position = hit.point;
    }

    public override void StateExit()
    {
        base.StateEntry();
    }

    public override void JumpStart()
    {
        pc.modelAnim.SetTrigger("Jump");
    }

    public override void CrouchStart()
    {
        pc.modelAnim.SetTrigger("Fall");
    }

}
