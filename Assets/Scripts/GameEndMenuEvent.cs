using System;
using UnityEngine;

public class GameEndMenuEvent : MonoBehaviour
{
    public static event Action GameEndMenu;

    public void GameEndMenuPopup()
    {
        GameEndMenu?.Invoke();
    }
}
