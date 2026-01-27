using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public RectTransform dialogueBox;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }


    public void PlayDialogue(DialogueObject obj)
    {
        StartCoroutine(Dialogue(obj));
    }

    public IEnumerator Dialogue(DialogueObject obj)
    {
        dialogueBox.DOScale(1, 0.5f).SetEase(Ease.OutQuad);
        nameText.text = "name";
        yield return new WaitForSeconds(.5f);
        foreach (string s in obj.dialogue)
        {
            foreach(char c in s)
            {
                dialogueText.text += s;
                yield return new WaitForSeconds(0.01f);
                //if player has pressed A button, print all of this string and break loop
            }
            //wait until player presses A button (or space bar) then repeat loop until all dialogue is shown
        }
    }
}
