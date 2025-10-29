using System.Collections;
using System.Threading;
using UnityEngine;

public class MoveSlide : MoveModule
{
    PlayerController controller;

    [Space]
    [Header("Crouch & Slide")]
    [SerializeField] bool canSlide = true;
    bool crouching;
    [SerializeField] float slideAccel = 400;
    [SerializeField] float slideMaxSpeed = 400;
    [SerializeField] float slideTime = 400;
    [SerializeField] float slideCounterMovement = 0.2f;

    [SerializeField] float crouchDelay;
    [SerializeField] float crouchHeight = 1;
    bool canChangeState;
    float standardHeight = 2;
    Coroutine c_checkCrouchExit;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        standardHeight = controller.cl.height;
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
/*        StartCoroutine(C_CrouchStart());
*/    }

/*    IEnumerator C_CrouchStart()
    {
        if (c_checkCrouchExit != null)
        {
            StopCoroutine(c_checkCrouchExit);
            c_checkCrouchExit = null;
        }
        crouching = true;
        controller.cl.height = crouchHeight;
        controller.cl.center = new Vector3(0, -crouchHeight / 2, 0);
    }*/

    IEnumerator C_Slide()
    {
        float time = 0;
        if (controller.rb.linearVelocity.magnitude > 0.5f && canSlide) //if the player is moving, boost them forward
        {
            while (controller.grounded && time < slideTime)
            {
                controller.rb.AddForce(controller.orientation.transform.forward * slideAccel);
                time += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
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
