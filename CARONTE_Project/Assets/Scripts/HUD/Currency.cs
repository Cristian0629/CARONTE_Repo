using UnityEngine;

public class Currency : MonoBehaviour
{
    public static Currency Instance;

    public int Coins { get; private set; }
    public int SpecialCoins { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetAll()
    {
        Coins = 0;
        SpecialCoins = 0;

        if (UIHUD.Instance != null)
        {
            UIHUD.Instance.Refresh(Coins, SpecialCoins);
            UIHUD.Instance.ResetMeters();
        }
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        if (UIHUD.Instance != null)
            UIHUD.Instance.Refresh(Coins, SpecialCoins);
    }

    public void AddSpecialCoins(int amount)
    {
        SpecialCoins += amount;
        if (UIHUD.Instance != null)
            UIHUD.Instance.Refresh(Coins, SpecialCoins);
    }
}
