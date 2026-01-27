using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    public static event Action OnCollected;
    public float scaleSpeed = 4;
    ParticleSystem ps;
    bool collected = false;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        transform.DORotate(new Vector3(0, 360, 0), 2.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;
/*            OnCollected();
*/            StartCoroutine(Collected());
        }
    }
    IEnumerator Collected()
    {
        ps.Play();
        bool scaled = false;
        float sc = 1f;
        while (!scaled)
        {
            sc -= Time.deltaTime * scaleSpeed;
            transform.localScale = new Vector3(sc, sc, sc);
            yield return null;
            if (sc <= 0) scaled = true;
        }
    }
}
