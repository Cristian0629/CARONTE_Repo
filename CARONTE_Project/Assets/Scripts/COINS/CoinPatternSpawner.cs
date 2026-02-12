using UnityEngine;

public class CoinPatternSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PatternEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;
    }

    [Header("Patterns (prefabs)")]
    public PatternEntry[] patterns;

    [Header("Spawn timing")]
    public float minSpawnTime = 0.6f;
    public float maxSpawnTime = 1.2f;

    [Header("Spawn position")]
    public float spawnX = 12f;
    public float minY = -2.8f;
    public float maxY = 3.2f;

    [Header("One at a time")]
    public bool onlyOnePatternOnScreen = true;

    float timer;
    int lastIndex = -1;
    GameObject currentPattern; 

    void Start() => ResetTimer();

    void Update()
    {
        

        
        if (onlyOnePatternOnScreen && currentPattern != null)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        SpawnPattern();
        ResetTimer();
    }

    void ResetTimer()
    {
        float a = Mathf.Min(minSpawnTime, maxSpawnTime);
        float b = Mathf.Max(minSpawnTime, maxSpawnTime);
        timer = Random.Range(a, b);
    }

    void SpawnPattern()
    {
        if (patterns == null || patterns.Length == 0) return;

        int idx = PickWeightedIndexNoRepeat();
        if (idx < 0) return;

        GameObject prefab = patterns[idx].prefab;
        if (prefab == null) return;

        float y = Random.Range(minY, maxY);

        GameObject patternObj = Instantiate(prefab, new Vector3(spawnX, y, 0f), Quaternion.identity);

        
        currentPattern = patternObj;

        var mover = patternObj.GetComponent<CoinPatternMover>();
        if (mover == null) mover = patternObj.AddComponent<CoinPatternMover>();

        mover.extraSpeed = 0f;

        if (patternObj.GetComponent<DestroyOffscreen>() == null)
            patternObj.AddComponent<DestroyOffscreen>();

        lastIndex = idx;
    }

    int PickWeightedIndexNoRepeat()
    {
        float total = 0f;
        for (int i = 0; i < patterns.Length; i++)
        {
            if (patterns[i].prefab == null) continue;
            if (patterns[i].weight <= 0f) continue;
            if (patterns.Length > 1 && i == lastIndex) continue;
            total += patterns[i].weight;
        }

        if (total <= 0f)
        {
            for (int i = 0; i < patterns.Length; i++)
                if (patterns[i].prefab != null) return i;
            return -1;
        }

        float r = Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < patterns.Length; i++)
        {
            if (patterns[i].prefab == null) continue;
            if (patterns[i].weight <= 0f) continue;
            if (patterns.Length > 1 && i == lastIndex) continue;

            acc += patterns[i].weight;
            if (r <= acc) return i;
        }

        return -1;
    }
}
