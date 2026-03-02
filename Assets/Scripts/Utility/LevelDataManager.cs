using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestData
{
    public string name;
    public List<GameObject> objects;
    public bool enablesOtherQuest;
}

public class LevelDataManager : MonoBehaviour
{
    public List<LevelTransition> transitionPoints;
    [SerializeField] List<QuestData> data;
    [HideInInspector] public Dictionary<string, List<GameObject>> questStartTriggers;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        questStartTriggers = new();

        foreach (QuestData d in data)
        {
            questStartTriggers.Add(d.name, d.objects);
        }


        foreach (string s in CutsceneManager.instance.completedQuests)
        {
            //if we find the key in the dictionary, disable all its objects
            questStartTriggers.TryGetValue(s, out List<GameObject> data);
            if (data != null)
            {
                foreach (GameObject go in data)
                {
                    go.SetActive(false);
                }
            }
        }
    }

    public void CompleteQuest(string key)
    {
        CutsceneManager.instance.completedQuests.Add(key);
   }
}