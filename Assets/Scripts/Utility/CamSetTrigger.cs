using System;
using Unity.Cinemachine;
using UnityEngine;

public class CamSetTrigger : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        cam.Priority = 100;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        cam.Priority = 0;
    }
}
