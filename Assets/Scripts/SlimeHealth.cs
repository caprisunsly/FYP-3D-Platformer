using System.Collections;
using UnityEngine;

public class SlimeHealth : AnimalHealth
{
    [SerializeField] GameObject smokePuff;
    protected override IEnumerator Hitstun(Transform instigator)
    {
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce((transform.position - instigator.position + Vector3.up * 2).normalized * hitForce, ForceMode.Impulse);
        rb.AddTorque((transform.position - instigator.position + Vector3.up).normalized * hitForce / 10, ForceMode.Impulse);

        yield return new WaitForSeconds(.5f);
        Instantiate(smokePuff, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    protected override void Update()
    {
        
    }
}
