using System.Collections;
using UnityEngine;

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
    [field: SerializeField] public float moveSpeedAccel { get; private set; }
    [field: SerializeField] public float moveSpeedMax  { get; private set; }
    [field: SerializeField] public float gravity { get; private set; }
    public bool grounded { get; private set; }
    bool oldGrounded;
    [SerializeField] LayerMask whatIsGround;

    [SerializeField] float maxSlopeAngle = 35f;
    Vector3 slopeDirection;
    [SerializeField] float characterRotationSpeed;


    [Space]
    [Header("Jumping")]
    [SerializeField] int totalJumps;
    public int jumpsRemaining;

    [SerializeField] float coyoteTime;
    [SerializeField] float jumpBufferTime = .2f;

    //Sliding
    private Vector3 normalVector = Vector3.up;

    public Vector2 dir;

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
        //slow the player down if they are going above max speed (prevents diagonal movement at high speed)
        if (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.z) > moveSpeedMax && grounded)
        {
            rb.AddForce(moveSpeedAccel * new Vector3(-rb.linearVelocity.normalized.x, 0, -rb.linearVelocity.normalized.z));
        }
        //Find actual velocity relative to where camera is looking
        Vector2 mag = FindVelRelativeToLook();

        //slows the player down if they arent inputting anything
        CounterMovement(dir.x, dir.y, mag);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded) return;

        //Counter movement
        if (Mathf.Abs(mag.x) > 0.01f && Mathf.Abs(x) < 0.05f || (mag.x < -0.01f && x > 0) || (mag.x > 0.01f && x < 0))
        {
            rb.AddForce(moveSpeedAccel * orientation.transform.right * -mag.x * .175f);
        }
        if (Mathf.Abs(mag.y) > 0.01f && Mathf.Abs(y) < 0.05f || (mag.y < -0.01f && y > 0) || (mag.y > 0.01f && y < 0))
        {
            rb.AddForce(moveSpeedAccel * orientation.transform.forward * -mag.y * .175f);
        }
    }

    private void CameraOrientation()
    {
        orientation.transform.localRotation = Quaternion.Euler(0, playerCam.transform.localRotation.eulerAngles.y, 0);
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
                //
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
