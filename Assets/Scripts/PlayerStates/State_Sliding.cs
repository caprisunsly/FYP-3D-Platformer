using System.Collections;
using UnityEngine;

public class State_Sliding : Base_State
{
    [Header("Sliding")]
    [SerializeField] float slideAccel = 400;
    [SerializeField] float slideMaxSpeed = 400;
    [SerializeField] float slideTime = 400;
    [SerializeField] float jumpBufferTime;
    Vector2 dir;

    Coroutine c_sliding;

    float offset;
    float veer;
    bool canExit;

    public override void StateEntry()
    {
        base.StateEntry();
        veer = pc.dir.x;
        dir = pc.dir;
        offset = pc.cl.center.y;
        pc.cl.height = pc.crouchingHeight;
        pc.cl.center = new Vector3(0, -pc.crouchingHeight / 2 + offset, 0);
        c_sliding = StartCoroutine(C_Sliding());
    }

    public override void StateExit()
    {
        base.StateExit();

        pc.modelAnim.SetBool("Sliding", false);
        pc.cl.height = pc.standingHeight;
        pc.cl.center = new Vector3(0, offset, 0);
        StopCoroutine(c_sliding);
    }

    public override void Movement(Vector2 dir)
    {
        veer = dir.x;
    }

    IEnumerator C_Sliding()
    {
        float time = 0;
        pc.modelAnim.SetTrigger("Slide");
        pc.modelAnim.SetBool("Sliding", true);
        if (dir == Vector2.zero) dir = new Vector2(pc.rb.linearVelocity.x, pc.rb.linearVelocity.z).normalized; //prevents super slow slides when the player slides as they release movement keys
        while (time < slideTime || !canExit)
        {
            //if something is found, cant exit crouch due to low ceiling bugs.
            canExit = !Physics.BoxCast(transform.position, new Vector3(1, pc.crouchingHeight, 1), transform.up, Quaternion.identity, 2, pc.whatIsGround);

            dir = new Vector2(dir.x + veer, dir.y).normalized;
            Vector3 movement = Vector3.ClampMagnitude(pc.orientation.transform.forward * dir.y + pc.orientation.transform.right * dir.x, 1);

            //only apply the force if we're still below the max speed threshold
            if (pc.rb.linearVelocity.magnitude < slideMaxSpeed)
            {
                //Apply forces to move player
                pc.rb.AddForce(Vector3.ProjectOnPlane(movement, pc.slopeDirection).normalized * slideAccel);

            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        sm.ChangeState(sm.stateStanding);
    }

    public override void JumpStart()
    {
        StartCoroutine(C_JumpBuffer());
    }

    IEnumerator C_JumpBuffer()
    {
        float time = 0;
        while (time < jumpBufferTime)
        {
            if (canExit)
            {
                sm.ChangeState(sm.stateJumping);
                break;
            }

            yield return null;
            time += Time.deltaTime;
        }
    }

    public override void GroundedEnd()
    {
        sm.ChangeState(sm.stateFalling);
    }
}
