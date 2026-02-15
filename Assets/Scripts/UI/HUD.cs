using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TMP_Text coinCounter;
    [SerializeField] TMP_Text keyCounter;
    int coins;
    int keys;

    private void OnEnable()
    {
        Coin.OnCollected += CountCoin;
        Key.OnCollected += CountKey;
    }

    public int GetKeyCount() => keys;
    public void SetKeyCount(int count) => keys = count;

    private void OnDisable()
    {
        Coin.OnCollected -= CountCoin;
        Key.OnCollected -= CountKey;
    }

    void CountCoin()
    {
        coins++;
        coinCounter.text = "x " + coins.ToString();
    }

    void CountKey(string keyName)
    {
        keys++;
        keyCounter.text = "x  " + keys.ToString();
    }
}
