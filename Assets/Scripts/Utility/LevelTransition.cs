using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [NaughtyAttributes.Scene]
    [SerializeField] string scene;
    [SerializeField] int spawnTransitionIndex;

    public Transform startPoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CutsceneManager.instance.LevelTransition(scene, spawnTransitionIndex);
    }
}
