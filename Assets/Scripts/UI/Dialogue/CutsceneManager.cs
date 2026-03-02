using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;
    public RectTransform dialogueBox;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image speakerIcon;
    [SerializeField] Image proceedPrompt;
    [SerializeField] CanvasGroup fadeCanvas;

    public Dictionary<UpgradeType, bool> collectedUpgrades = new();
    public List<string> completedQuests = new();
    [field: SerializeField] public HUD hud { get; private set; }
    bool buttonPressed = false;

    public Dictionary<string, Sprite[]> controlSchemeSprites = new();

    public static event Action OnCutsceneStarted;
    public static event Action OnCutsceneEnded;
    public static event Action UpgradeUpdate;

    [Header("Player Spawning")]

    public int playerSpawnIndex = -1;
    [SerializeField] GameObject playerPrefab;
    public bool inCutscene { get; private set; }


    [SerializeField] Menu pauseMenu, deathMenu, winMenu;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else Destroy(gameObject);

        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            collectedUpgrades.Add(upgradeType, false);
        }
        
    }
    private void OnEnable()
    {
        PlayerInputHandler.AdvanceDialogue += ButtonPress;
        PlayerInputHandler.ChangeControlScheme += ControlSchemeChanged;
        SceneManager.sceneLoaded += SceneLoaded;
        Upgrade.OnCollectUpgrade += CollectUpgrade;
        PlayerHealth.OnPlayerDied += DeathMenu;
        GameEndMenuEvent.GameEndMenu += GameWinMenu;
    }

    private void OnDisable()
    {
        PlayerInputHandler.AdvanceDialogue -= ButtonPress;
        PlayerInputHandler.ChangeControlScheme -= ControlSchemeChanged;
        SceneManager.sceneLoaded -= SceneLoaded;
        Upgrade.OnCollectUpgrade -= CollectUpgrade;
        PlayerHealth.OnPlayerDied -= DeathMenu;
        GameEndMenuEvent.GameEndMenu -= GameWinMenu;
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
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

    public void CustomCutscene(bool started)
    {
        if (started)
        {
            OnCutsceneStarted?.Invoke();
            inCutscene = true;
        }
        else
        {
            OnCutsceneEnded?.Invoke();
            inCutscene = false;
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
        speakerIcon.sprite = data.obj.icon;
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

    public void LevelTransition(string scene, int spawnIndex)
    {
        StartCoroutine(LoadNewScene(scene));
        playerSpawnIndex = spawnIndex;
    }

    IEnumerator LoadNewScene(string scene)
    {
        fadeCanvas.DOFade(1, .8f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(scene);
    }

    private void SceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        LevelDataManager manager = FindFirstObjectByType<LevelDataManager>();
        if (playerSpawnIndex == -1)
        {
            Instantiate(playerPrefab, manager.transform.position, Quaternion.identity, null);
            StartCoroutine(FadeIn());
            return;
        }
        if (playerSpawnIndex == -2)
        {
            StartCoroutine(FadeIn());
            return;
        }
        StartCoroutine(FadeIn());
        LevelTransition spawn = manager.transitionPoints[playerSpawnIndex];
        GameObject player = Instantiate(playerPrefab, spawn.startPoint.position, Quaternion.identity, null);
        player.GetComponentInChildren<PlayerController>().MoveCutscene(spawn.startPoint.forward);
    }

    IEnumerator FadeIn()
    {
        pauseMenu.ResetMenu();
        deathMenu.ResetMenu();
        yield return new WaitForSecondsRealtime(.25f);
        FindFirstObjectByType<CinemachineBrain>().ActiveBlend = null;
        fadeCanvas.DOFade(0, .8f);
    }

    void CollectUpgrade(UpgradeType upgradeType)
    {
        collectedUpgrades[upgradeType] = true;
        UpgradeUpdate?.Invoke();
    }

    public void PauseGame()
    {
        if (deathMenu.gameObject.activeInHierarchy) return;
        if (pauseMenu.gameObject.activeInHierarchy)
        {
            pauseMenu.FadeCanvas(0);
            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.FadeCanvas(1);
    }

    public void DeathMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        deathMenu.gameObject.SetActive(true);
        deathMenu.FadeCanvas(1);
    }

    public void GameWinMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        winMenu.gameObject.SetActive(true);
        winMenu.FadeCanvas(1);
    }

    public void MarkQuestComplete(string s)
    {
        completedQuests.Add(s);
    }
}
