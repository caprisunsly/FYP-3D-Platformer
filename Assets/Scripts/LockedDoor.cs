using UnityEngine;
using UnityEngine.Events;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] int keysRequired;
    public UnityEvent openEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int keys = CutsceneManager.instance.hud.GetKeyCount();
            if (keys >= keysRequired)
            {
                CutsceneManager.instance.hud.SetKeyCount(keys - keysRequired);
                openEvent?.Invoke();
            }
        }
    }

   
    public void DoorOpen()
    {
/*        Destroy(gameObject);
*/    }
}
