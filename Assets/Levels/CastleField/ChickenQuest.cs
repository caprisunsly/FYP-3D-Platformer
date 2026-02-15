using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct CutsceneData
{
    public DialogueObject obj;
    public CinemachineCamera cam;
    public UnityEvent postSceneEvent;
}

public class ChickenQuest : MonoBehaviour
{
    //on interact, pick dialogue option based on current quest state
    [SerializeField] int numChickens;
    int chickensInBarn;
    int chickensInYard;
    [SerializeField] CutsceneData goodEnd, badEnd;
    [SerializeField] GameObject pointsEffect;
    [SerializeField] TriggerEvent barnTrigger;

    private void OnEnable()
    {
        barnTrigger.TriggerEnterPos += ChickenBarnEffect;
    }

    private void OnDisable()
    {
        barnTrigger.TriggerEnterPos -= ChickenBarnEffect;
    }

    public void ChickenBarn(int mod)
    {
        chickensInBarn += mod;
        if (chickensInBarn >= numChickens)
        {
            CutsceneManager.instance.PlayCutscene(goodEnd);
            CompleteQuest();
        }
    }

    public void ChickenBarnEffect(Vector3 pos)
    {
        Instantiate(pointsEffect, pos, Quaternion.identity);
        //play a sound
    }

    public void ChickenYard(int mod)
    {
        chickensInYard += mod;
        if (chickensInYard <= 0 && chickensInBarn <= 0)
        {
            StartCoroutine(AllChickensOut());
        }
    }

    //prevents early completion from the navmesh agent being deactivated
    IEnumerator AllChickensOut()
    {
        yield return new WaitForSeconds(0.25f);
        if (chickensInYard <= 0 && chickensInBarn <= 0)
        {
            CutsceneManager.instance.PlayCutscene(badEnd);
            CompleteQuest();
        }
    }

    void CompleteQuest()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }
}
