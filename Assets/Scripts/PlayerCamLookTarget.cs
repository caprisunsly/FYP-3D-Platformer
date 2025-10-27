using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamLookTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float deadzoneDown, deadzoneUp;
    [SerializeField] float offset;

    void Update()
    {
        float posCompare = target.position.y + offset - transform.position.y;
        float appliedY = transform.position.y;
        //if the position is outside the deadzone, lerp targetY towards it
        if (posCompare < deadzoneDown || posCompare > deadzoneUp) appliedY = Mathf.Lerp(transform.position.y, target.position.y + offset, Time.deltaTime);

        transform.position = new Vector3(target.position.x, appliedY, target.position.z);
    }
}
