using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] TMP_Text coinCounter;
    [SerializeField] TMP_Text keyCounter;
    [SerializeField] Slider healthSlider;

    [SerializeField] Color green, yellow, red;
    [SerializeField] TMP_Text lifeCounter;
    [SerializeField] Image sliderFill;

    int coins;
    int keys;
    bool spawned;

    public static event Action CoinCollectKey;

    private void OnEnable()
    {
        Coin.OnCollected += CountCoin;
        Key.OnCollected += CountKey;
        PlayerHealth.OnHealthChange += HealthChange;
    }

    public int GetKeyCount() => keys;
    public void SetKeyCount(int count) => keys = count;
    public int GetCurHealth() => Mathf.FloorToInt(healthSlider.value);

    private void OnDisable()
    {
        Coin.OnCollected -= CountCoin;
        Key.OnCollected -= CountKey;
        PlayerHealth.OnHealthChange -= HealthChange;
    }

    void CountCoin()
    {
        coins++;
        coinCounter.text = "x " + coins.ToString();
        if (coins > 100 && !spawned)
        {
            spawned = true;
            //spawn a key on the player
        }
    }

    void CountKey()
    {
        keys++;
        keyCounter.text = "x  " + keys.ToString();
    }

    void HealthChange(int hp)
    {
        switch (hp)
        {
            case 3: sliderFill.color = green;
                break;
            case 2: sliderFill.color = yellow;
                break;
            default: sliderFill.color = red;
                break;
        }
        healthSlider.value = hp;
        lifeCounter.text = hp.ToString();
    }
}
