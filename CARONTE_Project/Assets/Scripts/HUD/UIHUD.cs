using TMPro;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    public static UIHUD Instance;

    [Header("TMP References")]
    [SerializeField] private TMP_Text metersText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text specialCoinsText;

    [Header("Meters Options")]
    [SerializeField] private Transform player;
    [SerializeField] private float metersMultiplier = 1f;

    [Header("If your player doesn't move in X (world scrolls)")]
    [SerializeField] private bool useSpeedBasedMeters = true;
    [SerializeField] private float worldSpeed = 5f;

    private float startX;
    private float metersAccum;

    // ✅ valor actual de metros (para GameOver)
    public float CurrentMeters { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (player != null)
            startX = player.position.x;

        // ✅ En vez de poner 0, sincroniza con Currency si existe
        UpdateCurrencyUIFromCurrency();
    }

    void Update()
    {
        UpdateMeters();

        // ✅ NUEVO: sincroniza monedas normales y especiales desde Currency (source of truth)
        UpdateCurrencyUIFromCurrency();
    }

    void UpdateMeters()
    {
        float meters;

        if (useSpeedBasedMeters)
        {
            float s = (GameSpeed.Instance != null) ? GameSpeed.Instance.Speed : worldSpeed;
            metersAccum += s * Time.deltaTime;

            meters = metersAccum;
        }
        else
        {
            if (player == null) return;
            meters = Mathf.Max(0f, (player.position.x - startX) * metersMultiplier);
        }

        CurrentMeters = meters;

        if (metersText != null)
            metersText.text = $"{meters:0} m";
    }

    // ✅ NUEVO: siempre muestra lo que realmente hay en Currency
    private void UpdateCurrencyUIFromCurrency()
    {
        if (Currency.Instance == null) return;

        if (coinsText != null)
            coinsText.text = Currency.Instance.Coins.ToString();

        if (specialCoinsText != null)
            specialCoinsText.text = Currency.Instance.SpecialCoins.ToString();
    }

    // Lo dejamos por compatibilidad (si otros scripts lo llaman, seguirá funcionando)
    public void Refresh(int coins, int specialCoins)
    {
        if (coinsText != null) coinsText.text = coins.ToString();
        if (specialCoinsText != null) specialCoinsText.text = specialCoins.ToString();
    }

    public void ResetMeters()
    {
        metersAccum = 0f;
        CurrentMeters = 0f;
        if (player != null) startX = player.position.x;
    }
}
