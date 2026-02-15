using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;
    public RectTransform dialogueBox;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    [SerializeField] Image proceedPrompt;
    [field: SerializeField] public HUD hud { get; private set; }
    bool buttonPressed = false;

    public Dictionary<string, Sprite[]> controlSchemeSprites = new();

    public static event Action OnCutsceneStarted;
    public static event Action OnCutsceneEnded;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        PlayerInputHandler.AdvanceDialogue += ButtonPress;
        PlayerInputHandler.ChangeControlScheme += ControlSchemeChanged;
    }

    private void OnDisable()
    {
        PlayerInputHandler.AdvanceDialogue -= ButtonPress;
        PlayerInputHandler.ChangeControlScheme -= ControlSchemeChanged;
    }

    void ButtonPress()
    {
        buttonPressed = true;
    }

    void ControlSchemeChanged(string s)
    {
        switch (s)
        {
            case "KeyboardMouse":
                {
                    
                    break;
                }
            case "XB":
                {

                    break;
                }
        }
    }

    public void PlayCutscene(CutsceneData data)
    {
        if (data.obj != null) StartCoroutine(DialogueCutscene(data));
        else StartCoroutine(TimerCutscene(data));
    }

    public IEnumerator DialogueCutscene(CutsceneData data)
    {
        data.cam.Priority = 100;
        buttonPressed = false;
        dialogueBox.DOScale(1, 0.5f).SetEase(Ease.OutQuad);
        dialogueText.text = "";
        nameText.text = data.obj.speakerName;
        OnCutsceneStarted?.Invoke();
        yield return new WaitForSeconds(.5f);
        foreach (string s in data.obj.dialogue)
        {
            dialogueText.text = "";
            //hide next dialogue prompt
            foreach (char c in s)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.01f);
                if (buttonPressed) break;
            }
            buttonPressed = false;
            proceedPrompt.transform.DOScale(1, 0.5f);
            dialogueText.text = s;
            yield return new WaitForSeconds(0.1f);
            buttonPressed = false;
            yield return new WaitUntil(() => buttonPressed == true);
            buttonPressed = false;
            proceedPrompt.transform.DOScale(0, 0.25f);
            //wait until player presses A button (or space bar) then repeat loop until all dialogue is shown
        }
        OnCutsceneEnded.Invoke();
        dialogueBox.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
        data.cam.Priority = 0;
        if (data.postSceneEvent != null) data.postSceneEvent.Invoke();
    }

    public IEnumerator TimerCutscene(CutsceneData data)
    {
        data.cam.Priority = 100;
        OnCutsceneStarted?.Invoke();
        yield return new WaitForSeconds(2f);
        OnCutsceneEnded.Invoke();
        data.cam.Priority = 0;
        if (data.postSceneEvent != null) data.postSceneEvent.Invoke();
    }
}
