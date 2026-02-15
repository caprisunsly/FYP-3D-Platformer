using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class AnimalHealth : MonoBehaviour, IDamageable
{
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    public float hitForce;
    [SerializeField] Animator anim;
    bool grounded;
    Coroutine c_Hitstun;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
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
        rb.constraints = RigidbodyConstraints.None;
        if (c_Hitstun != null) StopCoroutine(c_Hitstun);
        c_Hitstun = StartCoroutine(Hitstun(instigator));
    }

    protected virtual IEnumerator Hitstun(Transform instigator)
    {
        yield return new WaitForFixedUpdate();
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce((transform.position - instigator.position + Vector3.up * 2).normalized * hitForce, ForceMode.Impulse);
        rb.AddTorque((transform.position - instigator.position + Vector3.up).normalized * hitForce / 10, ForceMode.Impulse);

        grounded = false;
        yield return new WaitForSeconds(1f);
        while (!grounded)
        {
            yield return new WaitForSeconds(.2f);
        }
        yield return new WaitForSeconds(2f);
        if (grounded)
        {
            rb.angularVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
            transform.DORotate(Vector3.zero, .5f);
            transform.DOMoveY(transform.position.y + 1, .25f).SetLoops(1, LoopType.Yoyo);
            yield return new WaitForSeconds(0.65f);
            agent.enabled = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            c_Hitstun = null;
        }
        else
        {
            StartCoroutine(Hitstun(instigator));
        }
    }
}
