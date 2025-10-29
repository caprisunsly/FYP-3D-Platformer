using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public Transform playerCam { get; private set; }
    [field: SerializeField] public Transform orientation { get; private set; }
    [SerializeField] Transform playerModel;
    public CapsuleCollider cl { get; private set; }
    public Rigidbody rb { get; private set; }

    [Header("Movement")]
    [field: SerializeField] public float moveSpeedAccel { get; private set; }
    [SerializeField] float moveSpeedMax = 20;
    [SerializeField] float gravity;
    public bool grounded { get; private set; }
    [SerializeField] LayerMask whatIsGround;

    [SerializeField] float counterMovement = 0.175f;
    float threshold = 0.01f;
    [SerializeField] float maxSlopeAngle = 35f;
    [SerializeField] float characterRotationSpeed;


    [Space]
    [Header("Jumping")]
    [SerializeField] int totalJumps;
    private bool readyToJump = true;
    int jumpsRemaining = 1;
    float jumpCooldown = 0.25f;

    [SerializeField] float jumpForce = 550f;
    [SerializeField] float jumpCancelForce = 200f;

    bool moving = true, jumping, crouching;
    [SerializeField] float coyoteTime;
    [SerializeField] float jumpBufferTime = .2f;
    Coroutine c_movement, c_jumpCancel, c_jumpBuffer;

    //Some multipliers for other scripts to use
    float multiplier = 1f;
    [HideInInspector] public float multiplierV = 1f, multiplierH = 1f;

    //Sliding
    private Vector3 normalVector = Vector3.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cl = GetComponent<CapsuleCollider>();
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
        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.LookRotation(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z)), characterRotationSpeed);
        if (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.z) > moveSpeedMax && grounded)
        {
            rb.AddForce(moveSpeedAccel * 1.25f * new Vector3(-rb.linearVelocity.normalized.x, 0, -rb.linearVelocity.normalized.z));
        }
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

            if (!grounded) multiplier = .7f;

            //Apply forces to move player
            rb.AddForce(orientation.transform.forward * appliedDir.y * moveSpeedAccel * multiplier * multiplierV);
            rb.AddForce(orientation.transform.right * appliedDir.x * moveSpeedAccel * multiplier * multiplierH);

            if (dir == Vector2.zero && rb.linearVelocity.magnitude == 0) moving = false;
            yield return new WaitForFixedUpdate();
        }
        c_movement = null;
    }

    public void JumpStart()
    {
        jumping = true;
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
        if (time < jumpBufferTime)
        {
            Jump();
            if (!jumping) //if the player isnt holding jump after a buffered jump
            {
                Invoke(nameof(JumpCancel), 0.15f);
            }
        }
        c_jumpBuffer = null;
    }

    private void Jump()
    {
        if (grounded && readyToJump && jumpsRemaining > 0)
        {
            readyToJump = false;
            jumpsRemaining--;

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
        while (rb.linearVelocity.y > 0)
        {
            rb.AddForce(new Vector3(0, -rb.linearVelocity.y, 0) * jumpCancelForce);
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

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeedAccel * orientation.transform.right * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeedAccel * orientation.transform.forward * -mag.y * counterMovement);
        }

/*        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.linearVelocity.x, 2) + Mathf.Pow(rb.linearVelocity.z, 2))) > moveSpeedMax)
        {
            float fallspeed = rb.linearVelocity.y;
            Vector3 n = rb.linearVelocity.normalized * moveSpeedMax;
            rb.linearVelocity = new Vector3(n.x, fallspeed, n.z);
        }*/
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

        float magnitude = rb.linearVelocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    bool oldGrounded;
    private float moveSpeedAccel1;

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
