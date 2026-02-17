using DG.Tweening;
using System.Collections;
using UnityEngine;

public class UI_BobRotate : MonoBehaviour
{
    [SerializeField] float bobAmount = .5f;
    [SerializeField] float bobTime = 2f;
    [SerializeField] float swayAmount = 1f;
    [SerializeField] float swayTime = 5f;
    [SerializeField] float startDelay = 0f;

    private void Start()
    {
        StartCoroutine(BobAndSway());
    }

    IEnumerator BobAndSway()
    {
        yield return new WaitForSeconds(startDelay);

        RectTransform t = GetComponent<RectTransform>();
        t.rotation = Quaternion.Euler(new Vector3(0, 0, -swayAmount));
        if (bobAmount != 0) t.DOMoveY(transform.position.y + bobAmount, bobTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetUpdate(true);
        if (swayAmount != 0) t.DOLocalRotate(new Vector3(0, 0, swayAmount), swayTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetUpdate(true);
    }
}
