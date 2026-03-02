using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    Transform respawnPoint;
    [SerializeField] ParticleSystem smokePuff;
    [SerializeField] CinemachineCamera deathCam;
    [SerializeField] CinemachineCamera gameCam;
    [SerializeField] GameObject playerModel;
    PlayerController pc;
    PlayerStateManager sm;
    float blendTime = .75f;
    [SerializeField] float respawnTime = .75f;

    [SerializeField] int maxHealth = 3;
    int curHealth;

    bool invincible;

    public static event Action<int> OnHealthChange;
    public static event Action OnPlayerDied;
    public static event Action<bool> HealthAtMax;

    private void OnEnable()
    {
        RespawnPoint.RespawnSet += SetRespawn;
        blendTime = Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time;
    }

    private void OnDisable()
    {
        RespawnPoint.RespawnSet -= SetRespawn;
    }

    void Start()
    {
        pc = GetComponent<PlayerController>();
        sm = GetComponent<PlayerStateManager>();

        curHealth = CutsceneManager.instance.hud.GetCurHealth();
        if (curHealth != 0)
        {
            HealthAtMax?.Invoke(true);
            return;
        }
        HealthAtMax?.Invoke(false);
        curHealth = maxHealth;
        OnHealthChange?.Invoke(curHealth);
    }

    public void Damage(int damage, Transform instigator, IDamageable.DamageTypes damageType)
    {
        if (damageType == IDamageable.DamageTypes.LevelHazard)
        {
            PerformRespawn();
            return;
        }

        //if the player was at max health before the damage
        if (curHealth == maxHealth) HealthAtMax?.Invoke(false);

        //if damaged, make the player invincible and make them enter the damaged state
        if (damage > 0)
        {
            if (invincible) return;
            invincible = true;
            StartCoroutine(C_Invincibility());
            pc.knockbackDir = new Vector3(transform.position.x - instigator.position.x, 0, transform.position.z - instigator.position.z).normalized;
            sm.ChangeState(sm.stateDamaged);
        }

        curHealth -= damage;

        //if the player is dead
        if (curHealth <= 0)
        {
            curHealth = 0;
            StartCoroutine(PlayerDeath());
        }
        //if the player would be at or equal to max health
        if (curHealth >= maxHealth)
        {
            curHealth = maxHealth;
            HealthAtMax?.Invoke(true);
        }

        OnHealthChange?.Invoke(curHealth);
    }

    IEnumerator C_Invincibility()
    {
        yield return new WaitForSeconds(0.75f);
        invincible = false;
    }

    IEnumerator PlayerDeath()
    {
        CutsceneManager.instance.CustomCutscene(true);
        //move the death cinecamera to the pos of the main camera & set higher priority
        deathCam.transform.position = gameCam.transform.position;
        deathCam.Target.LookAtTarget = playerModel.transform;
        deathCam.Priority = 100;
        playerModel.SetActive(false);
        smokePuff.Play();
        yield return new WaitForSeconds(1f);
        OnPlayerDied?.Invoke();
    }   //show kill screen

    public void SetRespawn(Transform respawn)
    {
        respawnPoint = respawn;
    }

    public void PerformRespawn()
    {
        StartCoroutine(Respawn());
    }
       
    IEnumerator Respawn()
    {
        CutsceneManager.instance.CustomCutscene(true);
        //move the death cinecamera to the pos of the main camera & set higher priority
        deathCam.transform.position = gameCam.transform.position;
        deathCam.Target.LookAtTarget = playerModel.transform;
        deathCam.Priority = 100;
        playerModel.SetActive(false);
        smokePuff.Play();
        yield return new WaitForSeconds(blendTime);
        deathCam.Target.LookAtTarget = null;
        yield return new WaitForFixedUpdate(); //wait for fixed update before teleporting player to avoid weird physics stuff
        transform.position = respawnPoint.position;
        yield return new WaitForSeconds(respawnTime);
        deathCam.Priority = 0;
        yield return new WaitForSeconds(blendTime * .7f);
        smokePuff.Play();
        playerModel.SetActive(true);
        CutsceneManager.instance.CustomCutscene(false);
    }
}
