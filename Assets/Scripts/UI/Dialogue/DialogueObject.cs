using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue Object")]
public class DialogueObject : ScriptableObject
{
    public string speakerName;
    public Sprite icon;
    [TextArea(3, 5)]
    public List<string> dialogue;
}
