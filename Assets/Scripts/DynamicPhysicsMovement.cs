using UnityEngine;

public class DynamicPhysicsMovement : MonoBehaviour
{
    private Vector3 parentPosLastFrame = Vector3.zero;
    Rigidbody rb;

    void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.AddForce((parentPosLastFrame - transform.parent.position) * 100);
        parentPosLastFrame = transform.parent.position;
    }
}
