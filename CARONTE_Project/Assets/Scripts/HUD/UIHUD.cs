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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (player != null)
            startX = player.position.x;

        Refresh(0, 0);
    }

    void Update()
    {
        UpdateMeters();
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

        if (metersText != null)
            metersText.text = $"{meters:0} m";
    }

    public void Refresh(int coins, int specialCoins)
    {
        if (coinsText != null) coinsText.text = coins.ToString();
        if (specialCoinsText != null) specialCoinsText.text = specialCoins.ToString();
    }

    public void ResetMeters()
    {
        metersAccum = 0f;
        if (player != null) startX = player.position.x;
    }
}
