using System.Collections;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public Base_State currentState { get; private set; }
    [field: SerializeField] public Base_State stateStanding { get; private set; }
    [field: SerializeField] public Base_State stateSliding { get; private set; }
    [field: SerializeField] public Base_State stateJumping { get; private set; }
    [field: SerializeField] public Base_State stateFalling { get; private set; }
    [field: SerializeField] public Base_State stateLedgeHang { get; private set; }
    [field: SerializeField] public Base_State stateAirDive { get; private set; }
    Coroutine c_transitionTimer, c_waitForTransition;

    PlayerController pc;

    private void OnEnable()
    {
        PlayerController.EnterGrounded += EnterGround;
        PlayerController.ExitGrounded += ExitGround;
    }

    private void OnDisable()
    {
        PlayerController.EnterGrounded -= EnterGround;
        PlayerController.ExitGrounded -= ExitGround;
    }

    private void Start()
    {
        GameObject runner = new GameObject("CoroutineRunner");
        runner.AddComponent<CoroutineRunner>();
        
        pc = GetComponent<PlayerController>();
        currentState = stateStanding;
        currentState.StateEntry(pc, this);
    }

    private void Update()
    {
        currentState.StateUpdate();
    }

    private void FixedUpdate()
    {
        currentState.StateFixedUpdate();
    }

    void EnterGround()
    {
        currentState.GroundedStart();
    }

    void ExitGround()
    {
        currentState.GroundedEnd();
    }

    public void ChangeState(Base_State newState)
    {
        if (c_waitForTransition != null)
        {
            StopCoroutine(c_waitForTransition);
        }

        if (currentState.transitionTime != 0 && c_transitionTimer != null)
        {
            c_waitForTransition = StartCoroutine(C_WaitForTransition(newState));
            Debug.Log(currentState.name);
            return;
        }
        currentState.StateExit();
        currentState = newState;
        currentState.StateEntry(pc, this);
        Debug.Log(currentState + ": " + (Time.time - t));
        t = Time.time;
        c_transitionTimer = StartCoroutine(C_TransitionTimer(currentState.transitionTime));
    }

    /*    public void StopTransition()
        {
            StopCoroutine(c_transitionTimer);
            c_transitionTimer = null;
        }
    */
    float t = 0;
    IEnumerator C_WaitForTransition(Base_State newState)
    {
        yield return new WaitUntil(() => c_transitionTimer == null);
        currentState.StateExit();
        currentState = newState;
        currentState.StateEntry(pc, this);
        Debug.Log(currentState + ": " + (Time.time - t));
        t = Time.time;
        c_transitionTimer = StartCoroutine(C_TransitionTimer(currentState.transitionTime));
        c_waitForTransition = null;
    }

    IEnumerator C_TransitionTimer(float time)
    {
        yield return new WaitForSeconds(time);
        c_transitionTimer = null;
    }
}
