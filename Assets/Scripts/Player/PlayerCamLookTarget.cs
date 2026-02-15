using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerCamLookTarget : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] Transform lookTarget;
    [SerializeField] float hardLimitDown;
    [SerializeField] float offset;
    [SerializeField] float damping;
    [SerializeField] float timeToRecenter;
    float mult;
    [SerializeField] CinemachineOrbitalFollow CMFollow;
    CinemachineInputAxisController IAController;
    Coroutine c_centerCam;

    private void Start()
    {
        IAController = CMFollow.GetComponent<CinemachineInputAxisController>();
    }

    private void OnEnable()
    {
        CutsceneManager.OnCutsceneStarted += DisableCamLook;
        CutsceneManager.OnCutsceneEnded += EnableCamLook;
    }

    private void OnDisable()
    {
        CutsceneManager.OnCutsceneStarted -= DisableCamLook;
        CutsceneManager.OnCutsceneEnded -= EnableCamLook;
    }

    void EnableCamLook()
    {
        IAController.enabled = true;
    }

    void DisableCamLook()
    {
        IAController.enabled = false;
    }

    void Update()
    {
        float posCompare = followTarget.position.y + offset - transform.position.y;
        float appliedY = transform.position.y;
        //if the position is outside the deadzone, lerp targetY towards it
        if (transform.position.y - followTarget.position.y + offset > hardLimitDown) mult = 5;
        else mult = 1;
        appliedY = Mathf.Lerp(transform.position.y, followTarget.position.y + offset, (1 + damping) * Time.deltaTime * mult);

        transform.position = new Vector3(followTarget.position.x, appliedY, followTarget.position.z);
        transform.rotation = lookTarget.rotation;


        /* how to make the super cool camera

        rotate with the player on a timer. so after the camera hasnt been manually rotated for a while, rotate it automatically until a manual rotation is performed
        rotate to behind the player when they do a ledge grab?



        */
    }
    /*
        public void CameraInput(CallbackContext context)
        {
            float i = context.ReadValue<Vector2>().magnitude;
            if (i != 0)
            {
                if (c_timer != null)
                {
                    StopCoroutine(c_timer);
                    c_timer = null;
                    CMFollow.HorizontalAxis.Recentering.Enabled = false;
                    CMFollow.VerticalAxis.Recentering.Enabled = false;
                }
                return;
            }

            if (c_timer == null) c_timer = StartCoroutine(C_RecenterTimer());
        }*/

    public void CameraLock(CallbackContext context)
    {
        if (!context.started) return;
        if (c_centerCam != null) StopCoroutine(c_centerCam);
        c_centerCam = StartCoroutine(RecenterCam());
    }

    IEnumerator RecenterCam()
    {
        CMFollow.HorizontalAxis.Recentering.Enabled = true;
        CMFollow.VerticalAxis.Recentering.Enabled = true;
        yield return new WaitForSeconds(timeToRecenter);
        CMFollow.HorizontalAxis.Recentering.Enabled = false;
        CMFollow.VerticalAxis.Recentering.Enabled = false;
        c_centerCam = null;
    }
/*
    IEnumerator C_RecenterTimer()
    {
        yield return new WaitForSeconds(timeUntilRecenter);
        CMFollow.HorizontalAxis.Recentering.Enabled = true;
        CMFollow.VerticalAxis.Recentering.Enabled = true;
    }*/

}
