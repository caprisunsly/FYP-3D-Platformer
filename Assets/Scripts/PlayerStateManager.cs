using System.Collections;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public Base_State currentState { get; private set; }
    public State_Standing stateStanding { get; private set; }
    public State_Crouching stateCrouching { get; private set; }
    public State_Sliding stateSliding { get; private set; }
    public State_Jumping stateJumping { get; private set; }
    public State_Falling stateFalling { get; private set; }
    public State_LedgeHang stateLedgeHang { get; private set; }
    public float transitionTimer { get; private set; }
    Coroutine c_transitionTimer, c_waitForTransition;

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
        stateStanding = GetComponent<State_Standing>();
/*        stateCrouching = GetComponent<State_Crouching>();
*/        stateSliding = GetComponent<State_Sliding>();
        stateJumping = GetComponent<State_Jumping>();
        stateFalling = GetComponent<State_Falling>();
        stateLedgeHang = GetComponent<State_LedgeHang>();
        currentState = stateStanding;
        currentState.StateEntry();
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

        currentState.StateExit();
        currentState = newState;
        currentState.StateEntry();
        Debug.Log(currentState + ": " + (Time.time - t));
        t = Time.time;

        /*        if (c_waitForTransition != null)
                {
                    StopCoroutine(c_waitForTransition);
                }
                c_waitForTransition = StartCoroutine(C_WaitForTransition(newState));*/
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
        currentState.StateEntry();
        Debug.Log(currentState + ": " + (Time.time - t));
        t = Time.time;
        c_transitionTimer = StartCoroutine(C_TransitionTimer());
        c_waitForTransition = null;
    }

    IEnumerator C_TransitionTimer()
    {
        yield return new WaitForSeconds(transitionTimer);
        c_transitionTimer = null;
    }
}
