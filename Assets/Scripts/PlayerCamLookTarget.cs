using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamLookTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float hardLimitDown;
    [SerializeField] float offset;
    [SerializeField] float damping;
    float mult;

    void Update()
    {
        float posCompare = target.position.y + offset - transform.position.y;
        float appliedY = transform.position.y;
        //if the position is outside the deadzone, lerp targetY towards it
        if (transform.position.y - target.position.y + offset > hardLimitDown) mult = 5;
        else mult = 1;
        appliedY = Mathf.Lerp(transform.position.y, target.position.y + offset, (1 + damping) * Time.deltaTime * mult);

        transform.position = new Vector3(target.position.x, appliedY, target.position.z);
    }
}
