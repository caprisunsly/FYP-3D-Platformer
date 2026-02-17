using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    Vector3 closedPos;
    [SerializeField] GameObject doorObj;

    [SerializeField] bool startOpen = false;
    [SerializeField] float moveTime = 0.5f;
    [SerializeField] float delay = 0.5f;

    [Space]

    [SerializeField] Vector3 openPos = new Vector3(0, 1, 0);

    bool open = false;

    private void Start()
    {
        closedPos = transform.position;
        if (startOpen)
        {
            transform.position = closedPos + openPos;
            open = true;
        }
    }

    public void Toggle()
    {
        if (open) Close();
        else Open();
    }

    public void Close()
    {
        if (!open) return;
        open = false;
        StartCoroutine(MoveDoor(closedPos));
    }

    public void Open()
    {
        if (open) return;
        open = true;
        StartCoroutine(MoveDoor(closedPos + openPos));
    }

    IEnumerator MoveDoor(Vector3 pos)
    {
        yield return new WaitForSeconds(delay);
        doorObj.transform.DOMove(pos, moveTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(transform.InverseTransformPoint(transform.position + openPos) + GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
    }
}
