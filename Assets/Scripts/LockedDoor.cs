using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] int keysRequired;
    public UnityEvent openEvent;
    public TMP_Text number;

    private void Start()
    {
        number.text = keysRequired.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int keys = CutsceneManager.instance.hud.GetKeyCount();
            if (keys >= keysRequired)
            {
                openEvent?.Invoke();
                GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

/*    private void OnValidate()
    {
        number.text = keysRequired.ToString();
    }*/
}
