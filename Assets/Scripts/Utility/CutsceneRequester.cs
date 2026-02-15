using Unity.Cinemachine;
using UnityEngine;

public class CutsceneRequester : MonoBehaviour
{
    [SerializeField] bool playOnAwake;
    [SerializeField] bool doOnce;

    [SerializeField] CutsceneData data;

    private void Awake()
    {
        if (playOnAwake) PlayDialogue();
    }

    public void PlayDialogue()
    {
        CutsceneManager.instance.PlayCutscene(data);
        if (doOnce) gameObject.SetActive(false);
    }
}
