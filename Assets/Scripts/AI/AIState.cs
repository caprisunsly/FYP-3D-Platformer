using UnityEngine;

[RequireComponent(typeof(AIStateManager))]
public abstract class AIState : MonoBehaviour
{
    public bool active = true;
    [field: SerializeField] public float delay { get; private set; } = 0;
    protected AIStateManager manager { get; private set; }

    private void Awake()
    {
        manager = GetComponent<AIStateManager>();
    }

    public abstract bool CalculateTarget(out Vector3 target);
    public abstract void StateFixedUpdate();

    public void SetManager(AIStateManager m)
    {
        manager = m;
    }
}
