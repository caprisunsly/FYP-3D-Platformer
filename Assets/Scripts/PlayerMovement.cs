using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] CinemachineCamera cam;

    [Header("Movement")]
    [SerializeField] float maxSpeed;
    [SerializeField] float moveForce;
    [SerializeField] float jumpForce;
    float jumpCancelForce;
    bool moving;


    //coroutine references
    Coroutine c_Movement;
    Coroutine c_JumpCancel;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpCancelForce = jumpForce / 4;
    }

    public void Move(Vector3 input)
    {
        moving = true;
        if (c_Movement != null)
        {
            StopCoroutine(c_Movement);
            c_Movement = null;
        }
        c_Movement = StartCoroutine(C_Movement(input));
    }

    public IEnumerator C_Movement(Vector3 input)
    {
        while (moving)
        {
            Quaternion rot = Quaternion.FromToRotation(input, cam.transform.forward);
            input = rot * input;
            rb.AddForce(input * (moveForce * (1 - rb.linearVelocity.magnitude / maxSpeed)));
            Debug.Log(rb.linearVelocity.magnitude);
            yield return null;
        }
    }
#region jumping
    public void JumpStart()
    {   
        if (c_JumpCancel != null)
        {
            StopCoroutine(c_JumpCancel);
            c_JumpCancel = null;
        }

        rb.AddRelativeForce(Vector3.up * jumpForce);
    }

    public void JumpEnd()
    {
        if (c_JumpCancel != null) return;
        c_JumpCancel = StartCoroutine(C_JumpCancel());
    }

    public IEnumerator C_JumpCancel()
    {
        while (rb.linearVelocity.y > 0)
        {
            rb.AddRelativeForce(-Vector3.up * jumpCancelForce);
            yield return null;
        }
    }
    #endregion
}
