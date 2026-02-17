using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Pickup : MonoBehaviour
{
    public float scaleSpeed = 4;
    ParticleSystem ps;
    bool collected = false;
    public UnityEvent pickupEvent;
    public static event Action<string, string, Mesh, Material> PlayCollectAnim;
    [SerializeField] bool playAnim;

    [NaughtyAttributes.ShowIf("playAnim")]
    public string pickupName = "Default Name";
    [NaughtyAttributes.ShowIf("playAnim")]
    public string pickupDesc = "YOU GOT A KEY!";
    [NaughtyAttributes.ShowIf("playAnim")]
    public MeshFilter mesh;

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;
            CollectEvent(other.gameObject);
            StartCoroutine(Collected());
        }
    }

    public virtual void CollectEvent(GameObject player)
    {
        if (playAnim) PlayCollectAnim?.Invoke(pickupName, pickupDesc, mesh.mesh, mesh.GetComponent<MeshRenderer>().material);
        else pickupEvent.Invoke();
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
        if (playAnim) yield return new WaitForSeconds(2.5f);
        CollectComplete();
    }

    public virtual void CollectComplete()
    {
        if (playAnim) pickupEvent.Invoke();
        Destroy(gameObject);
    }
}
