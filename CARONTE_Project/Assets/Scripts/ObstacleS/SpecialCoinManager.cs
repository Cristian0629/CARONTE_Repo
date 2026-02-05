using UnityEngine;
using TMPro;

[DefaultExecutionOrder(10000)]
public class SpecialCoinManager : MonoBehaviour
{
    public static SpecialCoinManager Instance { get; private set; }

    public int target = 15;

    public TMP_Text specialCoinsText;
    public string specialCoinsTextName = "SpecialCoinsText";

    public int Current { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // ✅ IMPORTANTE: reinicia cada vez que abres el juego
        Current = 0;

        FindHUDIfNeeded();
        ForceHUD();
    }

    void Update()
    {
        if (specialCoinsText == null)
            FindHUDIfNeeded();
    }

    void LateUpdate()
    {
        ForceHUD();
    }

    public void Add(int amount)
    {
        if (Current >= target) return;

        Current = Mathf.Clamp(Current + amount, 0, target);
        ForceHUD();

        if (Current >= target)
            Debug.Log($"✅ VICTORIA (DEV): Special Coins {Current}/{target}");
    }

    void ForceHUD()
    {
        if (specialCoinsText != null)
            specialCoinsText.text = $"{Current}/{target}";
    }

    void FindHUDIfNeeded()
    {
        if (specialCoinsText != null) return;

        GameObject go = GameObject.Find(specialCoinsTextName);
        if (go != null)
            specialCoinsText = go.GetComponent<TMP_Text>();
    }
}
