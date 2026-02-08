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
        Debug.Log($"[Currency] SpecialCoins now = {SpecialCoins}");

        if (UIHUD.Instance != null)
            UIHUD.Instance.Refresh(Coins, SpecialCoins);
    }


    // ✅ NUEVO: gastar monedas (para +1 Life)
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (Coins < amount) return false;

        Coins -= amount;

        if (UIHUD.Instance != null)
            UIHUD.Instance.Refresh(Coins, SpecialCoins);

        return true;
    }
}
