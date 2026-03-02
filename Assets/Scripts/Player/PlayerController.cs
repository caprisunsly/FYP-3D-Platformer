using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public Transform playerCam { get; private set; }
    [field: SerializeField] public Transform orientation { get; private set; }
    [field: SerializeField] public Transform playerModel { get; private set; }
    [field: SerializeField] public Animator modelAnim { get; private set; }
    [field: SerializeField] public Transform ledgeDetection { get; private set; }
    float defaultColliderRadius;

    PlayerInputHandler inputHandler;

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
    [SerializeField] Vector3 closeFloorCheckArea;
    [field: SerializeField] public LayerMask whatIsGround { get; private set; }
    [field: SerializeField] public LayerMask whatIsWall { get; private set; }
    [field: SerializeField] public bool canJump { get; set; }
    [field: SerializeField] public bool canDoubleJump { get; set; }
    [field: SerializeField] public bool canDive { get; set; }

    [SerializeField] float maxSlopeAngle = 35f;
    [field: SerializeField] public float minGroundDistance { get; private set; }
    public Vector3 slopeDirection { get; private set; } = Vector3.up;
    [SerializeField] float characterRotationSpeed;


    [Header("Jumping")]
    [field: SerializeField] public int totalJumps { get; private set; }
    [field: SerializeField] public int totalAirSwipes { get; private set; }
    [field: SerializeField] public int jumpsRemaining { get; set; }
    [field: SerializeField] public int divesRemaining { get; set; }
    [field: SerializeField] public int airSwipesRemaining { get; set; }
    float gravityMult = 1;
    int doRotate = 1;

    [SerializeField] float coyoteTime;

    [field: SerializeField] public Vector2 gatedDir { get; set; } = Vector2.zero; //to be used for movement. locks the player to 8 directions, more predictable movement
    [field: SerializeField] public Vector2 ungatedDir { get; set; } = Vector2.zero; //to be used for extra cases like the air dive. more precise, wont screw the player over


    public bool floorClose { get; private set; }
    Coroutine c_coyote;

    public static event Action EnterGrounded;
    public static event Action ExitGrounded;

    public bool crouchHeld, jumpHeld, moving;

    public float jumpedFrom { get; set; }//store where the player jumped from
    public Vector3 knockbackDir { get; set; }//store the direction the player should be knocked back when hit.

    [SerializeField] TMP_Text bigText;
    [SerializeField] TMP_Text smallText;
    [SerializeField] MeshFilter meshF;
    [SerializeField] MeshRenderer meshR;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cl = GetComponent<CapsuleCollider>();
        defaultColliderRadius = cl.radius;
    }

    private void OnEnable()
    {
        Pickup.PlayCollectAnim += CollectSpecial;
    }
    private void OnDisable()
    {
        Pickup.PlayCollectAnim -= CollectSpecial;
    }


    void Start()
    {
/*        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
        StartCoroutine(C_Movement());
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        CameraOrientation();
    }

    private void LateUpdate()
    {
        CheckGrounded();
    }

    void CollectSpecial(string name, string description, Mesh model, Material mat)
    {
        StartCoroutine(CollectAnim(name, description, model, mat));
    }

    IEnumerator CollectAnim(string name, string description, Mesh model, Material mat)
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
        smallText.text = name;
        bigText.text = description;
        meshF.mesh = model;
        meshR.material = mat;
        modelAnim.SetBool("KeyCollect", true);
        inputHandler.InputDisable();
        doRotate = 0;
        playerModel.transform.DORotateQuaternion(Quaternion.LookRotation(Vector3.ProjectOnPlane(playerCam.position - transform.position, slopeDirection)), 0.4f);
        yield return new WaitForSeconds(2.6f);
        doRotate = 1;
        modelAnim.SetBool("KeyCollect", false);
        inputHandler.InputEnable();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void MoveCutscene(Vector3 moveDirection)
    {
        StartCoroutine(C_MoveCutscene(moveDirection));
    }

    IEnumerator C_MoveCutscene(Vector3 direction)
    {
        yield return new WaitForFixedUpdate();
        CutsceneManager.instance.CustomCutscene(true);
        Movement(new Vector2(direction.x, direction.z), new Vector2(direction.x, direction.z));
        FindFirstObjectByType<CinemachineBrain>().ActiveBlend = null;
        yield return new WaitForSeconds(1f);
        Movement(Vector2.zero, Vector2.zero);
        CutsceneManager.instance.CustomCutscene(false);
    }

    public void SetColliderRadius(float radius) => cl.radius = radius;
    public void ResetColliderRadius() => cl.radius = defaultColliderRadius;

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

    public void Movement(Vector2 gatedDir, Vector2 ungatedDir)
    {
/*        if (c_movement != null)
        {
            StopCoroutine(c_movement);
            c_movement = null;
        }
        moving = true;
        c_movement = StartCoroutine(C_Movement(gatedDir));*/
        this.gatedDir = gatedDir;
        this.ungatedDir = ungatedDir;
        modelAnim.SetInteger("InputXZ", Mathf.FloorToInt(this.ungatedDir.magnitude + .99f));
    }

    public float multiplier = 1;

    private IEnumerator C_Movement()
    {
        while (moving)
        {
            //Find actual velocity relative to where camera is looking
            Vector2 mag = FindVelRelativeToLook();

            //slows the player down if they arent inputting anything
            /*            CounterMovement(dir.x, dir.y, mag);
            */
            //If speed is larger than maxspeed, cancel out the input so player doesn't go over max speed
            Vector2 appliedDir = ungatedDir.normalized;

            if (ungatedDir.x > 0 && mag.x > curSpeedMax) appliedDir.x = 0;
            if (ungatedDir.x < 0 && mag.x < -curSpeedMax) appliedDir.x = 0;
            if (ungatedDir.y > 0 && mag.y > curSpeedMax) appliedDir.y = 0;
            if (ungatedDir.y < 0 && mag.y < -curSpeedMax) appliedDir.y = 0;

            //Apply forces to move player
            Vector3 movement = Vector3.ClampMagnitude(orientation.transform.forward * appliedDir.y + orientation.transform.right * appliedDir.x, 1);

            //Apply forces to move player
            rb.AddForce(Vector3.ProjectOnPlane(movement.normalized, slopeDirection) * curSpeedAccel * multiplier);

/*            if (dir == Vector2.zero && rb.linearVelocity.magnitude == 0) moving = false;
*/            yield return new WaitForFixedUpdate();
        }
/*        c_movement = null;
*/    }
    private void FixedUpdate()
    {
        rb.AddForce(gravity * gravityMult * -slopeDirection); //gravity force
        Vector3 movement = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        modelAnim.SetFloat("SpeedXZ", (Mathf.Abs(movement.magnitude) / curSpeedMax) + .2f);
        if (movement.magnitude < 0.05f) movement = playerModel.forward;
        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(movement, slopeDirection)), characterRotationSpeed * doRotate);
        //slow the player down if they are going above max speed (prevents diagonal movement at high speed)
        if (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.z) > curSpeedMax)
        {
            rb.AddForce(curSpeedAccel * new Vector3(-rb.linearVelocity.normalized.x, 0, -rb.linearVelocity.normalized.z));
        }

        //slows the player down if they arent inputting anything
        if (ungatedDir.magnitude == 0) FrictionForce();
        //reset wall contact here, as fixed update runs before collision stay
        contactingWall = false;
    }

    private void FrictionForce()
    {
        if (!grounded) return;

        Vector3 vel = rb.linearVelocity;

        //Counter movement. This causes some funky stuff when the player jumps currently
        if (Mathf.Abs(vel.x) > 0.01f)
        {
            rb.AddForce(curSpeedDecel * new Vector3(-rb.linearVelocity.x, 0, 0));
        }
        else if (Mathf.Abs(vel.x) <= 0.01f && Mathf.Abs(vel.x) > 0) rb.linearVelocity = new Vector3(0, vel.y, vel.z);
        if (Mathf.Abs(vel.z) > 0.01f)
        {
            rb.AddForce(curSpeedDecel * new Vector3(0, 0, -rb.linearVelocity.z));
        }
        //changed to rb reference for the edge case that both happen at the same time. Wouldnt want this to overrwrite the change the other has made with the value of vel
        else if (Mathf.Abs(vel.x) <= 0.01f && Mathf.Abs(vel.x) > 0) rb.linearVelocity = new Vector3(rb.linearVelocity.x, vel.y, 0);
    }

    private void CameraOrientation()
    {
        orientation.transform.localRotation = Quaternion.Euler(0, Camera.main.transform.localRotation.eulerAngles.y, 0);
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

    public ContactPoint p;
    public bool contactingWall = false;

    private void OnCollisionStay(Collision hit)
    {
        if (!grounded)
        {
            if ((whatIsWall & (1 << hit.gameObject.layer)) == 0) return;

            p = hit.GetContact(0);
            if (p.normal.y < 0.5f) contactingWall = true;
            Debug.DrawRay(p.point, p.normal, Color.red, 1f);
/*            Debug.Log(hit.gameObject.name);
*/        }
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
        Gizmos.DrawWireCube(transform.position, closeFloorCheckArea);
    }

    bool floorGround;
    void CheckGrounded()
    {
        floorClose = false;
        floorGround = false;
        slopeDirection = Vector3.up;
        if (!overrideSlopeDirection)
        {
            if (Physics.CheckBox(transform.position, closeFloorCheckArea, Quaternion.identity, whatIsGround))
            {
                floorClose = true;

                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, groundedCheckArea.y*2, whatIsGround))
                {
                    if (IsFloor(info.normal))
                    {
                        slopeDirection = info.normal;
                        floorGround = true;
                    }
                }
            }
        }

/*        if (floorClose) modelAnim.SetBool("Standing", true);
        else modelAnim.SetBool("Standing", false);*/


        oldGrounded = grounded;
        //if raycast found a floor
        if (floorGround)
        {
            modelAnim.SetBool("Standing", true);
            if (Physics.CheckBox(transform.position, groundedCheckArea, Quaternion.identity, whatIsGround))
            {
                grounded = true;
                if (oldGrounded != grounded)
                {
                    jumpsRemaining = totalJumps;
                    EnterGrounded?.Invoke();
                    canJump = true;
/*                    canDoubleJump = true;
*/                    canDive = true;
                    jumpedFrom = 0;
                    divesRemaining = 1;
                    airSwipesRemaining = totalAirSwipes;
                    //counteract the slight slide down that is induced upon landing on a slope
                }
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
        yield return new WaitForSeconds(coyoteTime);
        modelAnim.SetBool("Standing", false);
        grounded = false;
        ExitGrounded.Invoke();
        canJump = false;
    }
}
