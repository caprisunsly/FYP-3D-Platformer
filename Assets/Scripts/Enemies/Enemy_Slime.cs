using Unity.Cinemachine;
using UnityEngine;

public class Enemy_Slime : MonoBehaviour
{
    AIStateManager stateManager;
    AIS_Chase chaseState;
    AIS_Wander wanderState;
    [SerializeField] Animator anim;
    private void Start()
    {
        stateManager = GetComponent<AIStateManager>();
        chaseState = GetComponent<AIS_Chase>();
        wanderState = GetComponent<AIS_Wander>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            chaseState.player = other.transform;
            stateManager.ChangeState(chaseState);
            anim.SetTrigger("Chase");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stateManager.ChangeState(wanderState);
            anim.SetTrigger("Wander");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent(out IDamageable target);
/*            if (target != null) target.Damage(damage, parent);
*/        }
    }
}
