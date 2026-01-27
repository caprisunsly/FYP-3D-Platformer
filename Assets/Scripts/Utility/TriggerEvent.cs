using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public UnityEvent TriggerEnter;
    public UnityEvent TriggerExit;
    public string Tag;


    private void OnTriggerEnter(Collider other)
    {
        if (Tag == "" || other.CompareTag(Tag))
        {
            TriggerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Tag == "" || other.CompareTag(Tag))
        {
            TriggerExit.Invoke();
        }
    }
}
