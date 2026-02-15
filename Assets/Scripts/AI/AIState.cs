using UnityEngine;

[RequireComponent(typeof(AIStateManager))]
public abstract class AIState : MonoBehaviour
{
    public bool active = true;
    [field: SerializeField] public float delay { get; private set; } = 0;
    protected AIStateManager manager { get; private set; }
    [SerializeField] float speed = 5;
    [SerializeField] float acceleration = 1000;

    private void Awake()
    {
        manager = GetComponent<AIStateManager>();
    }

    public abstract bool CalculateTarget(out Vector3 target);
    public virtual void StateEnter()
    {
        manager.agent.speed = speed;
        manager.agent.acceleration = acceleration;
    }
    public abstract void StateUpdate();

    public void SetManager(AIStateManager m)
    {
        manager = m;
    }
}
