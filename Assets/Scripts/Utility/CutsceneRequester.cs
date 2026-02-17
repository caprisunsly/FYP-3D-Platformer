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
        if (doOnce) data.postSceneEvent.AddListener(Disable);
        CutsceneManager.instance.PlayCutscene(data);
    }

    void Disable()
    {
        this.enabled = false;
    }
}
