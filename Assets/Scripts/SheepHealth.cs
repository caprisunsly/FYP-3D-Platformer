using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AnimalHealth : MonoBehaviour, IDamageable
{
    NavMeshAgent agent;
    Rigidbody rb;
    public float hitForce;
    [SerializeField] Animator anim;
    bool grounded;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        anim.SetFloat("speed", agent.velocity.magnitude);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Vector3.Angle(Vector3.up, collision.contacts[0].normal) < 35) grounded = true;
    }

    public void Damage(int damage, Transform instigator)
    {
        agent.enabled = false;
        rb.isKinematic = false;
        rb.AddForce((transform.position - instigator.position + Vector3.up * 2).normalized * hitForce, ForceMode.Impulse);
        rb.AddTorque((transform.position - instigator.position + Vector3.up).normalized * hitForce / 10, ForceMode.Impulse);
        StartCoroutine(Hitstun());
    }

    IEnumerator Hitstun()
    {
        grounded = false;
        yield return new WaitForSeconds(1f);
        while (!grounded)
        {
            yield return new WaitForSeconds(.2f);
        }
        yield return new WaitForSeconds(2f);
        if (grounded)
        {
            agent.enabled = true;
            rb.isKinematic = true;
        }
        else
        {
            StartCoroutine(Hitstun());
        }
    }
}
