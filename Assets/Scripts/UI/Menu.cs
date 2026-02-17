using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [Scene]
    [SerializeField] string menuScene;
    [SerializeField] GameObject firstSelected;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void Resume()
    {
        FadeCanvas(0);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        CutsceneManager.instance.LevelTransition(menuScene, -1);
    }


    public void ReturnToMenu()
    {
        CutsceneManager.instance.LevelTransition(menuScene, -2);
    }

    public void Respawn()
    {
        CutsceneManager.instance.LevelTransition(SceneManager.GetActiveScene().name, -2);
    }

    public void FadeCanvas(float val)
    {
        cg.DOFade(val, 0.3f).SetUpdate(true).onComplete += () =>
        {
            if (val == 0) gameObject.SetActive(false);
        };
    }

    public void ResetMenu()
    {
        cg.alpha = 0;
        gameObject.SetActive(false);
    }


}
