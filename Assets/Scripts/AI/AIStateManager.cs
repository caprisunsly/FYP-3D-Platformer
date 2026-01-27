using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class AIStateManager : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; }
    AIState[] states;
    AIState currentState;
    bool pathing;
/*    public event EventHandler<StateChangeArgs> OnStateChanged;
*/    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        states = GetComponents<AIState>();
        foreach (AIState s in states) s.SetManager(this);
        ChangeState(states[0]);
    }

    public IEnumerator CalculateTarget()
    {
        pathing = true;
        if (currentState.CalculateTarget(out Vector3 target))
        {
            yield return new WaitForSeconds(currentState.delay);
            agent.SetDestination(target);
            Debug.DrawRay(target, Vector3.up, Color.blue, 1f);
        }
        pathing = false;
    }

    void ChangeState(AIState newState)
    {
        currentState = newState;
        StartCoroutine(CalculateTarget());
/*        OnStateChanged(this, new StateChangeArgs(currentState));
*/    }

    private void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !pathing)
        {
            StartCoroutine(CalculateTarget());
        }
    }
}

public class StateChangeArgs : EventArgs
{
    public AIState state;
    public StateChangeArgs( AIState state)
    {
        this.state = state;
    }
}
