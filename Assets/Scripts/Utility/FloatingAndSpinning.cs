using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FloatingAndSpinning : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Spin());
    }

    IEnumerator Spin()
    {
        yield return new WaitForFixedUpdate();
        transform.DOLocalMoveY(transform.localPosition.y + .5f, 2f).SetLoops(-1, LoopType.Yoyo);
        transform.DOLocalRotate(transform.localRotation.eulerAngles + new Vector3(0, 360, 0), 2.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }
}
