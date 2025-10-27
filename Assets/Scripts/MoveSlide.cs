using System.Collections;
using UnityEngine;

public class MoveSlide : MoveModule
{
    PlayerController controller;

    [Space]
    [Header("Crouch & Slide")]
    [SerializeField] bool canSlide = true;
    bool crouching;
    [SerializeField] float slideForce = 400;
    [SerializeField] float slideCounterMovement = 0.2f;
    [SerializeField] float crouchHeight = 1;
    float standardHeight = 2;
    Coroutine c_checkCrouchExit;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

 /*   private void FixedUpdate()
    {
        //Slow down sliding
        if (crouching)
        {
            controller.rb.AddForce(controller.moveSpeedAccel * -controller.rb.linearVelocity.normalized * slideCounterMovement);
            return;
        }
    }
*/
    public override void InputStarted()
    {
        if (c_checkCrouchExit != null)
        {
            StopCoroutine(c_checkCrouchExit);
            c_checkCrouchExit = null;
        }
        crouching = true;
        controller.cl.height = crouchHeight;
        controller.cl.center = new Vector3(0, -crouchHeight / 2, 0);
        if (controller.rb.linearVelocity.magnitude > 0.5f && canSlide) //if the player is moving, boost them forward
        {
            if (controller.grounded)
            {
                controller.rb.AddForce(controller.orientation.transform.forward * slideForce);
            }
        }
    }

    public override void InputCancelled()
    {
        if (c_checkCrouchExit != null)
        {
            StopCoroutine(c_checkCrouchExit);
        }
        c_checkCrouchExit = StartCoroutine(C_CheckCrouchExit());
    }

    IEnumerator C_CheckCrouchExit()
    {
        while (crouching)
        {
            yield return null;
            //check if there any objects where the player would stand
            if (Physics.Raycast(new Vector3(transform.position.y, transform.position.y - controller.cl.height / 2, transform.position.z), transform.up, standardHeight)) continue;

            //if there is, uncrouch
            crouching = false;
            controller.cl.height = standardHeight;
            controller.cl.center = Vector3.zero;
        }
    }
}
