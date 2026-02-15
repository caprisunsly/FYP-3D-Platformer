using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    public float scaleSpeed = 4;
    ParticleSystem ps;
    bool collected = false;
    public UnityEvent pickupEvent;

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
        transform.DOScale(1.25f, 2);
        transform.DOMoveY(transform.position.y + .5f, 2f).SetLoops(-1, LoopType.Yoyo);
        transform.DOLocalRotate(new Vector3(0, 360, 0), 2.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;
            CollectEvent();
            StartCoroutine(Collected());
        }
    }

    public virtual void CollectEvent()
    {
        pickupEvent.Invoke();
    }

    IEnumerator Collected()
    {
        if (ps != null) ps.Play();
        bool scaled = false;
        float sc = 1f;
        while (!scaled)
        {
            sc -= Time.deltaTime * scaleSpeed;
            transform.localScale = new Vector3(sc, sc, sc);
            yield return null;
            if (sc <= 0) scaled = true;
        }
        CollectComplete();
    }

    public virtual void CollectComplete()
    {
        Destroy(gameObject);
    }
}
