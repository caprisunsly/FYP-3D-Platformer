using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public Base_State currentState { get; private set; }
    public Base_State prevState { get; private set; }
    [field: SerializeField] public Base_State stateStanding { get; private set; }
    [field: SerializeField] public Base_State stateSliding { get; private set; }
    [field: SerializeField] public Base_State stateJumping { get; private set; }
    [field: SerializeField] public Base_State stateFalling { get; private set; }
    [field: SerializeField] public Base_State stateLedgeHang { get; private set; }
    [field: SerializeField] public Base_State stateAirDive { get; private set; }
    [field: SerializeField] public Base_State stateTailSwipe { get; private set; }
    [field: SerializeField] public Base_State stateWallBounce { get; private set; }
    [field: SerializeField] public Base_State stateDamaged { get; private set; }
    Coroutine c_transitionTimer, c_waitForTransition;

    public Dictionary<UpgradeType, bool> collectedUpgrades;


    PlayerController pc;

    private void OnEnable()
    {
        PlayerController.EnterGrounded += EnterGround;
        PlayerController.ExitGrounded += ExitGround;
        CutsceneManager.UpgradeUpdate += EvaluateUpgrades;
    }

    private void OnDisable()
    {
        PlayerController.EnterGrounded -= EnterGround;
        PlayerController.ExitGrounded -= ExitGround;
        CutsceneManager.UpgradeUpdate -= EvaluateUpgrades;
    }

    private void Start()
    {
        GameObject runner = new GameObject("CoroutineRunner");
        runner.AddComponent<CoroutineRunner>();
        
        pc = GetComponent<PlayerController>();
        currentState = stateStanding;
        currentState.StateEntry(pc, this);
        EvaluateUpgrades();
    }

    void EvaluateUpgrades()
    {
        collectedUpgrades = CutsceneManager.instance.collectedUpgrades;
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
        if (newState.upgradeType != UpgradeType.None)
        {
            //check if player has the upgrade to enter any state that requires an upgrade. uses a dictionary so vv fast :)
            if (!collectedUpgrades[newState.upgradeType]) return;
        }


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
/*        Debug.Log(currentState + " -> " + newState);
*/        prevState = currentState;
        currentState = newState;
        currentState.StateEntry(pc, this);
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
/*        Debug.Log(currentState + ": " + (Time.time - t));
*/        t = Time.time;
        c_transitionTimer = StartCoroutine(C_TransitionTimer(currentState.transitionTime));
        c_waitForTransition = null;
    }

    IEnumerator C_TransitionTimer(float time)
    {
        yield return new WaitForSeconds(time);
        c_transitionTimer = null;
    }
}
