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
    public bool pathing { get; private set; }
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
            if (!agent.enabled)
            {
                pathing = false;
                yield break;
            }
            agent.SetDestination(target);
            Debug.DrawRay(target, Vector3.up, Color.blue, 1f);
            agent.isStopped = false;
        }
        pathing = false;
    }

    public void ChangeState(AIState newState)
    {
        currentState = newState;
        currentState.StateEnter();
        StartCoroutine(CalculateTarget());
    }

    private void Update()
    {
        currentState.StateUpdate();
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
