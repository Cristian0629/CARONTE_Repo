using UnityEngine;

public class SpecialCoinSpawner : MonoBehaviour
{
    public GameObject specialCoinPrefab;

    [Header("Spawn position")]
    public float spawnX = 12f;
    public float minY = -2.2f;
    public float maxY = 2.8f;

    [Header("Limits")]
    public int maxOnScreen = 1;

    float timer;

    void Start() => ResetTimer();

    void Update()
    {
        if (specialCoinPrefab == null) return;

        // Si ya has ganado, no spawnear más
        if (Currency.Instance != null && Currency.Instance.SpecialCoins >= 25)
            return;

        // Limitar cuántas hay a la vez
        if (maxOnScreen > 0 && CountSpecialCoinsInScene() >= maxOnScreen)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Spawn();
        ResetTimer();
    }

    void ResetTimer()
    {
        int sc = (Currency.Instance != null) ? Currency.Instance.SpecialCoins : 0;

        // FASES
        float minT, maxT;

        if (sc < 15)
        {
            minT = 6f; maxT = 10f;
        }
        else if (sc < 35)
        {
            minT = 4f; maxT = 8f;
        }
        else
        {
            minT = 3f; maxT = 6f;
        }

        timer = Random.Range(minT, maxT);
    }

    void Spawn()
    {
        float y = Random.Range(minY, maxY);
        Instantiate(specialCoinPrefab, new Vector3(spawnX, y, 0f), Quaternion.identity);
    }

    int CountSpecialCoinsInScene()
    {
#if UNITY_2023_1_OR_NEWER
        return FindObjectsByType<SpecialCoinPickup>(FindObjectsSortMode.None).Length;
#else
        return FindObjectsOfType<SpecialCoinPickup>().Length;
#endif
    }
}

