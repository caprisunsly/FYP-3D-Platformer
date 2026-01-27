using System.Collections.Generic;
using UnityEngine;

public class DialogueObject : ScriptableObject
{
    [TextArea(3, 5)]
    public List<string> dialogue;
}
