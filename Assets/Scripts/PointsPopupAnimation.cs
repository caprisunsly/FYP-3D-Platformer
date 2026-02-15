using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PointsPopupAnimation : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.Euler(90, 180, 0);
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        transform.DOScale(.2f, 0.3f);
        transform.DOMoveY(transform.position.y + 5, 1.2f);
        yield return new WaitForSeconds(.9f);
        transform.DOScale(0, 0.3f);
    }
}
