using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform hhhh;
    [field: SerializeField] public Transform playerCam { get; private set; }
    [field: SerializeField] public Transform orientation { get; private set; }
    [field: SerializeField] public Transform playerModel { get; private set; }
    [field: SerializeField] public Animator modelAnim { get; private set; }
    [field: SerializeField] public Transform ledgeDetection { get; private set; }

    public CapsuleCollider cl { get; private set; }
    public Rigidbody rb { get; private set; }
    [field: SerializeField] public float standingHeight { get; private set; } = 2f;
    [field: SerializeField] public float crouchingHeight { get; private set; } = 1f;
    

    [Header("Movement")]
    [field: SerializeField] public float curSpeedAccel { get; private set; }
    [field: SerializeField] public float curSpeedDecel  { get; private set; }
    [field: SerializeField] public float curSpeedMax  { get; private set; }
    [field: SerializeField] public float gravity { get; private set; }

    public bool grounded { get; private set; } = false;
    bool overrideSlopeDirection;
    bool oldGrounded;
    [SerializeField] Vector3 groundedCheckArea;
    [field: SerializeField] public LayerMask whatIsGround { get; private set; }
    [field: SerializeField] public bool canJump { get; set; }
    [field: SerializeField] public bool canDive { get; set; }

    [SerializeField] float maxSlopeAngle = 35f;
    public Vector3 slopeDirection { get; private set; } = Vector3.up;
    [SerializeField] float characterRotationSpeed;


    [Header("Jumping")]
    [field: SerializeField] public int totalJumps { get; private set; }
    [field: SerializeField] public int jumpsRemaining { get; set; }

    float gravityMult = 1;
    int doRotate = 1;

    [SerializeField] float coyoteTime;    

    public Vector2 dir = Vector2.zero;

    bool touchingFloor, onSlope;
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

    public void SetSpeed(float max, float accel, float decel, float gravity, int rotationMult)
    {
        gravityMult = gravity;
        doRotate = rotationMult;

        if (max == -1 || accel == -1 || decel == -1) //if the state will handle movement
        {
            curSpeedMax = 10000000; //essentially uncapped max speed in this state
            curSpeedAccel = 0; //no acceleration
            curSpeedDecel = 0; //no deceleration
            return;
        }
        curSpeedMax = max;
        curSpeedAccel = accel;
        curSpeedDecel = decel;
    }

    public IEnumerator C_OverrideSlopeDirection()
    {
        overrideSlopeDirection = true;
        yield return new WaitForSeconds(0.1f);
        overrideSlopeDirection = false;
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
        modelAnim.SetInteger("InputXZ", Mathf.RoundToInt(dir.magnitude));
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

            if (dir.x > 0 && mag.x > curSpeedMax) appliedDir.x = 0;
            if (dir.x < 0 && mag.x < -curSpeedMax) appliedDir.x = 0;
            if (dir.y > 0 && mag.y > curSpeedMax) appliedDir.y = 0;
            if (dir.y < 0 && mag.y < -curSpeedMax) appliedDir.y = 0;

            //Apply forces to move player
            Vector3 movement = Vector3.ClampMagnitude(orientation.transform.forward * appliedDir.y + orientation.transform.right * appliedDir.x, 1);

            //Apply forces to move player
            rb.AddForce(Vector3.ProjectOnPlane(movement, slopeDirection) * curSpeedAccel);

            if (dir == Vector2.zero && rb.linearVelocity.magnitude == 0) moving = false;
            yield return new WaitForFixedUpdate();
        }
        c_movement = null;
    }
    private void FixedUpdate()
    {
        rb.AddForce(gravity * gravityMult * -slopeDirection); //extra gravity force
        Vector3 movement = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        modelAnim.SetFloat("SpeedXZ", (Mathf.Abs(movement.magnitude) / curSpeedMax) + .2f);
        if (movement.magnitude < 0.05f) movement = playerModel.forward;
        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(movement, slopeDirection)), characterRotationSpeed * doRotate);
        //slow the player down if they are going above max speed (prevents diagonal movement at high speed)
        if (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.z) > curSpeedMax && grounded)
        {
            rb.AddForce(curSpeedAccel * new Vector3(-rb.linearVelocity.normalized.x, 0, -rb.linearVelocity.normalized.z));
        }

        //slows the player down if they arent inputting anything
        if (dir.magnitude == 0) FrictionForce();
    }

    private void FrictionForce()
    {
        if (!grounded) return;

        Vector3 vel = rb.linearVelocity;

        //Counter movement. This causes some funky stuff when the player jumps currently
        if (Mathf.Abs(vel.x) > 0.01f)
        {
            rb.AddForce(curSpeedDecel * new Vector3(-vel.x, 0, 0) * .175f);
        }
        else if (Mathf.Abs(vel.x) < 0.01f && Mathf.Abs(vel.x) > 0) rb.linearVelocity = new Vector3(0, vel.y, vel.z);
        if (Mathf.Abs(vel.z) > 0.01f)
        {
            rb.AddForce(curSpeedDecel * new Vector3(0, 0, -vel.z) * .175f);
        }
        //changed to rb reference for the edge case that both happen at the same time. Wouldnt want this to overrwrite the change the other has made with the value of vel
        else if (Mathf.Abs(vel.x) < 0.01f && Mathf.Abs(vel.x) > 0) rb.linearVelocity = new Vector3(rb.linearVelocity.x, vel.y, 0);
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


    private void OnCollisionEnter(Collision collision)
    {
        if (whatIsGround != (whatIsGround | (1 << collision.gameObject.layer))) return;
        if (IsFloor(collision.GetContact(0).normal))
        {
            ContactPoint hit = collision.GetContact(0);
            hhhh.rotation = Quaternion.FromToRotation(collision.GetContact(0).normal, Vector3.up) * hhhh.rotation;
        }
    }

    IEnumerator C_NegateSlopeSlide()
    {
        Debug.Log("HERE!!!! !    " + rb.linearVelocity);
        gravityMult = 0;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        yield return new WaitForFixedUpdate();
        gravityMult = 1;

        

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, groundedCheckArea);
    }

    void CheckGrounded()
    {
        touchingFloor = false;
        slopeDirection = Vector3.up;
        if (!overrideSlopeDirection)
        {
            if (Physics.CheckBox(transform.position, groundedCheckArea, Quaternion.identity, whatIsGround))
            {
                touchingFloor = true;

                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, groundedCheckArea.y, whatIsGround))
                {
                    if (IsFloor(info.normal))
                    {
                        slopeDirection = info.normal;
                    }
                }
            }
        }

        oldGrounded = grounded;
        //if OnCollisionStay found a valid floor
        if (touchingFloor)
        {
            modelAnim.SetBool("Standing", true);
            grounded = true;
            if (oldGrounded != grounded)
            {
                jumpsRemaining = totalJumps;
                EnterGrounded.Invoke();
                canJump = true;
                canDive = true;
                //counteract the slight slide down that is induced upon landing
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
        }
    }

    private IEnumerator C_CoyoteTime()
    {
        modelAnim.SetBool("Standing", false);
        yield return new WaitForSeconds(coyoteTime);
        grounded = false;
        ExitGrounded.Invoke();
    }
}
