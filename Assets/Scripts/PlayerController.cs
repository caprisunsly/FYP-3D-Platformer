using System.Collections;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCam;
    [SerializeField] Transform orientation;
    CapsuleCollider cl;
    Rigidbody rb;

    [Header("Movement")]
    [SerializeField] float moveSpeedAccel = 4500;
    [SerializeField] float moveSpeedMax = 20;

    [SerializeField] float gravity;
    [SerializeField] bool grounded;
    [SerializeField] LayerMask whatIsGround;

    [SerializeField] float counterMovement = 0.175f;
    float threshold = 0.01f;
    [SerializeField] float maxSlopeAngle = 35f;

    [Space]
    [Header("Crouch & Slide")]
    [SerializeField] float slideForce = 400;
    [SerializeField] float slideCounterMovement = 0.2f;
    [SerializeField] float crouchHeight = 1;
    float standardHeight = 2;

    [Space]
    [Header("Jumping")]
    //Jumping
    private bool readyToJump = true;
    [SerializeField] int totalJumps;
    int jumpsRemaining = 1;
    float jumpCooldown = 0.25f;

    [SerializeField] float jumpForce = 550f;
    [SerializeField] float jumpCancelForce = 200f;

    bool moving = true, jumping, crouching;
    [SerializeField] float coyoteTime;
    [SerializeField] float jumpBufferTime = .2f;
    Coroutine c_movement, c_jumpCancel, c_jumpBuffer;

    //Sliding
    private Vector3 normalVector = Vector3.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cl = GetComponent<CapsuleCollider>();
        standardHeight = cl.height;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CameraOrientation();
    }
    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity); //extra gravity force
    }

    public void CrouchStart()
    {
        cl.height = crouchHeight;
        cl.center = new Vector3(0, -crouchHeight / 2, 0);
        if (rb.linearVelocity.magnitude > 0.5f) //if the player is moving, boost them forward when they slide
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    public void CrouchStop()
    {
        //check if there is enough height to stop crouching
        //if not, wait a frame and try again
        cl.height = standardHeight;
        cl.center = Vector3.zero;
    }


    public void Movement(Vector2 dir)
    {
        if (c_movement != null)
        {
            StopCoroutine(c_movement);
            c_movement = null;
        }
        moving = true;
        c_movement = StartCoroutine(C_Movement(dir));
    }

    private IEnumerator C_Movement(Vector2 dir)
    {
        while (moving)
        {
            //Find actual velocity relative to where camera is looking
            Vector2 mag = FindVelRelativeToLook();

            //Counteract sliding and sloppy movement
            CounterMovement(dir.x, dir.y, mag);

            //If speed is larger than maxspeed, cancel out the input so player doesn't go over max speed
            Vector2 appliedDir = dir;

            if (dir.x > 0 && mag.x > moveSpeedMax) appliedDir.x = 0;
            if (dir.x < 0 && mag.x < -moveSpeedMax) appliedDir.x = 0;
            if (dir.y > 0 && mag.y > moveSpeedMax) appliedDir.y = 0;
            if (dir.y < 0 && mag.y < -moveSpeedMax) appliedDir.y = 0;

            //Some multipliers
            float multiplier = 1f, multiplierV = 1f;

            // Movement in air
            if (!grounded)
            {

            }

            // Movement while sliding
            if (grounded && crouching) multiplierV = 0f; //prevents the player from moving forward and backward while sliding

            //Apply forces to move player
            rb.AddForce(orientation.transform.forward * appliedDir.y * moveSpeedAccel * multiplier * multiplierV);
            rb.AddForce(orientation.transform.right * appliedDir.x * moveSpeedAccel * multiplier);

            if (dir == Vector2.zero && rb.linearVelocity.magnitude == 0) moving = false;
            yield return new WaitForFixedUpdate();
        }
        c_movement = null;
    }

    /*        //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (crouching && grounded && readyToJump)
            {
                rb.AddForce(Vector3.down * 3000);
                return;
            }*/

    public void JumpStart()
    {
        if (c_jumpBuffer != null)
        {
            StopCoroutine(c_jumpBuffer);
            c_jumpBuffer = null;
        }
        c_jumpBuffer = StartCoroutine(JumpBuffer());
    }

    IEnumerator JumpBuffer()
    {
        float time = 0;
        while (!grounded)
        {
            time += Time.deltaTime;
            yield return null;
        }
        if (time < jumpBufferTime) Jump();
        c_jumpBuffer = null;
    }

    private void Jump()
    {
        if (grounded && readyToJump && jumpsRemaining > 0)
        {
            readyToJump = false;
            jumpsRemaining--;
            jumping = true;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce /* * .75 */, ForceMode.Impulse);
            //sends the player at the angle of the ground they are standing on
            /*            rb.AddForce(normalVector * jumpForce * 0.25f, ForceMode.Impulse);*/

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.linearVelocity;
            if (rb.linearVelocity.y < 0.5f)
                rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void JumpCancel()
    {
        jumping = false;
        if (c_jumpCancel != null)
        {
            StopCoroutine(c_jumpCancel);
            c_jumpCancel = null;
        }
        c_jumpCancel = StartCoroutine(C_JumpCancel());
    }

    public IEnumerator C_JumpCancel()
    {
        while (rb.linearVelocity.y > 0 && !jumping)
        {
            rb.AddForce(Vector3.down * jumpCancelForce);
            yield return new WaitForFixedUpdate();
        }
        c_jumpCancel = null;
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void CameraOrientation()
    {
        orientation.transform.localRotation = Quaternion.Euler(0, playerCam.transform.localRotation.eulerAngles.y, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeedAccel * -rb.linearVelocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeedAccel * orientation.transform.right * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeedAccel * orientation.transform.forward * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.linearVelocity.x, 2) + Mathf.Pow(rb.linearVelocity.z, 2))) > moveSpeedMax)
        {
            float fallspeed = rb.linearVelocity.y;
            Vector3 n = rb.linearVelocity.normalized * moveSpeedMax;
            rb.linearVelocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.linearVelocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    bool oldGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //can potentially hijack this for wall collisions later if wall jumping is implemented or something similar

        //Make sure we are only checking for walkable layers.
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        oldGrounded = grounded;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) 
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                normalVector = normal;
                if (oldGrounded != grounded)
                {
                    jumpsRemaining = totalJumps;
                    CancelInvoke(nameof(LeaveGround));
                }
                //stop the StopGrounded coroutine
                break;
            }
        }
        Invoke(nameof(LeaveGround), coyoteTime);
    }

    private void LeaveGround() //acts also as a coyote time.
    {
        grounded = false;
        //fire event since we arent grounded anymore. also acts like coyote time
    }
}
