using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public Transform playerCam { get; private set; }
    [field: SerializeField] public Transform orientation { get; private set; }
    [field: SerializeField] public Transform playerModel { get; private set; }
    public CapsuleCollider cl { get; private set; }
    public Rigidbody rb { get; private set; }
    [field: SerializeField] public float standingHeight { get; private set; } = 2f;
    [field: SerializeField] public float crouchingHeight { get; private set; } = 1f;
    

    [Header("Movement")]
    [field: SerializeField] public float defaultSpeedAccel { get; private set; }
    [field: SerializeField] public float defaultSpeedMax  { get; private set; }
    [field: SerializeField] public float gravity { get; private set; }

    public bool grounded { get; private set; } = false;
    bool oldGrounded;
    [SerializeField] LayerMask whatIsGround;

    [SerializeField] float maxSlopeAngle = 35f;
    Vector3 slopeDirection = Vector3.up;
    [SerializeField] float characterRotationSpeed;


    [Space]
    [Header("Jumping")]
    [SerializeField] int totalJumps;
    public int jumpsRemaining;

    [SerializeField] float coyoteTime;    

    public Vector2 dir = Vector2.zero;

    bool touchingFloor;
    Coroutine c_coyote, c_movement;

    public static event Action EnterGrounded;
    public static event Action ExitGrounded;

    public bool crouchHeld, jumpHeld, moving;

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

    private void LateUpdate()
    {
        CheckGrounded();
    }

    public void SetSpeed(float max, float accel)
    {
        if (max == -1 || accel == -1) //if the state will handle movement
        {
            defaultSpeedMax = 10000000; //essentially uncapped max speed in this state
            defaultSpeedAccel = 0; //no acceleration
            return;
        }
        defaultSpeedMax = max;
        defaultSpeedAccel = accel;
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
        this.dir = dir;
    }

    private IEnumerator C_Movement(Vector2 dir)
    {
        while (moving)
        {
            //Find actual velocity relative to where camera is looking
            Vector2 mag = FindVelRelativeToLook();

            //slows the player down if they arent inputting anything
            /*            CounterMovement(dir.x, dir.y, mag);
            */
            //If speed is larger than maxspeed, cancel out the input so player doesn't go over max speed
            Vector2 appliedDir = dir;

            if (dir.x > 0 && mag.x > defaultSpeedMax) appliedDir.x = 0;
            if (dir.x < 0 && mag.x < -defaultSpeedMax) appliedDir.x = 0;
            if (dir.y > 0 && mag.y > defaultSpeedMax) appliedDir.y = 0;
            if (dir.y < 0 && mag.y < -defaultSpeedMax) appliedDir.y = 0;

            //Apply forces to move player
            Vector3 movement = Vector3.ClampMagnitude(orientation.transform.forward * appliedDir.y + orientation.transform.right * appliedDir.x, 1);

            //Apply forces to move player
            rb.AddForce(Vector3.ProjectOnPlane(movement, slopeDirection).normalized * defaultSpeedAccel);

            if (dir == Vector2.zero && rb.linearVelocity.magnitude == 0) moving = false;
            yield return new WaitForFixedUpdate();
        }
        c_movement = null;
    }
    private void FixedUpdate()
    {
        rb.AddForce(-slopeDirection * gravity); //extra gravity force
        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.LookRotation(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z)), characterRotationSpeed);
        //slow the player down if they are going above max speed (prevents diagonal movement at high speed)
        if (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.z) > defaultSpeedMax && grounded)
        {
            rb.AddForce(defaultSpeedAccel * new Vector3(-rb.linearVelocity.normalized.x, 0, -rb.linearVelocity.normalized.z));
        }
        //Find actual velocity relative to where camera is looking
        Vector2 mag = FindVelRelativeToLook();

        //slows the player down if they arent inputting anything
        if (dir.magnitude == 0) FrictionForce(mag);
    }

    private void FrictionForce(Vector2 mag)
    {
        if (!grounded) return;

        /*        //if the player is moving, reduce their speed relative to their current velocity????
                Vector3 movement = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                if (movement.magnitude > .1)
                {
                    rb.AddForce(-movement.normalized * moveSpeedAccel * .15f);
                }*/


        //Counter movement. This causes some funky stuff when the player jumps currently
        if (Mathf.Abs(mag.x) > 0.01f && Mathf.Abs(dir.x) < 0.05f || (mag.x < -0.01f && dir.x > 0) || (mag.x > 0.01f && dir.x < 0))
        {
            rb.AddForce(defaultSpeedAccel * orientation.transform.right * -mag.x * .175f);
        }
        if (Mathf.Abs(mag.y) > 0.01f && Mathf.Abs(dir.y) < 0.05f || (mag.y < -0.01f && dir.y > 0) || (mag.y > 0.01f && dir.y < 0))
        {
            rb.AddForce(defaultSpeedAccel * orientation.transform.forward * -mag.y * .175f);
        }
    }

    private void CameraOrientation()
    {
        orientation.transform.localRotation = Quaternion.Euler(0, playerCam.transform.localRotation.eulerAngles.y, 0);
    }

    // Find the velocity relative to where the player is looking
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

    private void OnCollisionStay(Collision collision)
    {
        //can potentially hijack this for wall collisions later if wall jumping is implemented or something similar
        //Make sure we are only checking for walkable layers.
        if (whatIsGround != (whatIsGround | (1 << collision.gameObject.layer))) return;

        //Iterate through every collision
        foreach (var contact in collision.contacts)
        {
            //FLOOR
            if (IsFloor(contact.normal)) //is the normal of the contact point within the players walkable range
            {
                touchingFloor = true;
                return;
            }
        }
    }

    void CheckGrounded()
    {
        slopeDirection = Vector3.up;
        Debug.DrawRay(transform.position, Vector3.down * 1.1f, Color.red, .02f);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, 1.1f, whatIsGround))
        {
            if (IsFloor(info.normal))
            {
                slopeDirection = info.normal;
            }
        }


        oldGrounded = grounded;
        //if OnCollisionStay found a valid floor
        if (touchingFloor)
        {
            grounded = true;
            if (oldGrounded != grounded)
            {
                jumpsRemaining = totalJumps;
                EnterGrounded.Invoke();
            }
            if (c_coyote != null)
            {
                StopCoroutine(c_coyote);
                c_coyote = null;
            }
        }
        else
        {
            if (c_coyote == null) c_coyote = StartCoroutine(C_CoyoteTime());
            slopeDirection = Vector3.up;
        }
        touchingFloor = false;
    }

    private IEnumerator C_CoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTime);
        grounded = false;
        ExitGrounded.Invoke();
    }
}
