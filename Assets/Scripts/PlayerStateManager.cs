using System.Collections;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public Base_State currentState { get; private set; }
    public State_Standing stateStanding { get; private set; }
    public State_Crouching stateCrouching { get; private set; }
    public State_Sliding stateSliding { get; private set; }
    public float transitionTimer { get; private set; }
    Coroutine c_transitionTimer, c_waitForTransition;

    private void Start()
    {
        stateStanding = GetComponent<State_Standing>();
        stateCrouching = GetComponent<State_Crouching>();
        stateSliding = GetComponent<State_Sliding>();
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

    public void ChangeState(Base_State newState)
    {
        if (c_waitForTransition != null)
        {
            StopCoroutine(c_waitForTransition);
        }
        c_waitForTransition = StartCoroutine(C_WaitForTransition(newState));
    }

    IEnumerator C_WaitForTransition(Base_State newState)
    {
        yield return new WaitUntil(() => c_transitionTimer == null);
        currentState.StateExit();
        currentState = newState;
        currentState.StateEntry();
        c_transitionTimer = StartCoroutine(C_TransitionTimer());
        c_waitForTransition = null;
    }

    IEnumerator C_TransitionTimer()
    {
        yield return new WaitForSeconds(transitionTimer);
        c_transitionTimer = null;
    }
}
