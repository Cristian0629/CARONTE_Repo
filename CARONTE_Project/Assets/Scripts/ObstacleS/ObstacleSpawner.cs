using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PatternEntry
    {
        public GameObject prefab;     // patrón (puede ser 1 obstáculo o un grupo)
        [Min(0f)] public float weight = 1f;
    }

    [Header("Obstacle patterns (prefabs)")]
    public PatternEntry[] obstaclePatterns;

    [Header("Timing (difficulty scaled)")]
    public float easyMinSpawn = 1.6f;
    public float easyMaxSpawn = 2.2f;

    public float hardMinSpawn = 0.55f;
    public float hardMaxSpawn = 0.9f;


    [Header("Camera margins")]
    public float rightPadding = 1.5f;
    public float topPadding = 0.8f;
    public float bottomPadding = 0.8f;

    [Header("Lanes (optional, recommended)")]
    public bool useLanes = true;
    public float[] obstacleLanesY;

    float nextSpawn;
    int lastIndex = -1;

    void Start() => ScheduleNext();

    void Update()

    {

        nextSpawn -= Time.deltaTime;
        if (nextSpawn <= 0f)
        {
            Spawn();
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        float diff = 0f;

        if (GameSpeed_BG.Instance != null)
            diff = GameSpeed_BG.Instance.Difficulty01; // 0 → 1

        // Interpola entre fácil y difícil según la dificultad
        float minT = Mathf.Lerp(easyMinSpawn, hardMinSpawn, diff);
        float maxT = Mathf.Lerp(easyMaxSpawn, hardMaxSpawn, diff);

        nextSpawn = Random.Range(minT, maxT);
    }


    void Spawn()
    {
        if (obstaclePatterns == null || obstaclePatterns.Length == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float spawnX = tr.x + rightPadding;

        float bottom = bl.y + bottomPadding;
        float top = tr.y - topPadding;

        float y = useLanes ? PickSafeLaneY(bottom, top) : Random.Range(bottom, top);

        int idx = PickWeightedIndexNoRepeat();
        if (idx < 0) return;

        GameObject prefab = obstaclePatterns[idx].prefab;
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, new Vector3(spawnX, y, 0f), Quaternion.identity);

        // Asegura que el patrón entero se mueve con el juego
        var mover = obj.GetComponent<ObstacleMover>();
        if (mover == null) mover = obj.AddComponent<ObstacleMover>();
        mover.extraSpeed = 0f;

        // Se destruye cuando sale de pantalla
        if (obj.GetComponent<DestroyOffscreen>() == null)
            obj.AddComponent<DestroyOffscreen>();

        lastIndex = idx;
    }

    float PickSafeLaneY(float bottom, float top)
    {
        if (obstacleLanesY == null || obstacleLanesY.Length == 0)
            return Random.Range(bottom, top);

        // elige una lane que quede dentro de la “zona segura”
        int safeCount = 0;
        for (int i = 0; i < obstacleLanesY.Length; i++)
            if (obstacleLanesY[i] >= bottom && obstacleLanesY[i] <= top) safeCount++;

        if (safeCount == 0)
            return Mathf.Clamp(obstacleLanesY[Random.Range(0, obstacleLanesY.Length)], bottom, top);

        int pick = Random.Range(0, safeCount);
        for (int i = 0; i < obstacleLanesY.Length; i++)
        {
            if (obstacleLanesY[i] < bottom || obstacleLanesY[i] > top) continue;
            if (pick-- == 0) return obstacleLanesY[i];
        }

        return 0f;
    }

    int PickWeightedIndexNoRepeat()
    {
        float total = 0f;
        for (int i = 0; i < obstaclePatterns.Length; i++)
        {
            if (obstaclePatterns[i].prefab == null) continue;
            if (obstaclePatterns[i].weight <= 0f) continue;
            if (obstaclePatterns.Length > 1 && i == lastIndex) continue;
            total += obstaclePatterns[i].weight;
        }

        if (total <= 0f)
        {
            for (int i = 0; i < obstaclePatterns.Length; i++)
                if (obstaclePatterns[i].prefab != null) return i;
            return -1;
        }

        float r = Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < obstaclePatterns.Length; i++)
        {
            if (obstaclePatterns[i].prefab == null) continue;
            if (obstaclePatterns[i].weight <= 0f) continue;
            if (obstaclePatterns.Length > 1 && i == lastIndex) continue;

            acc += obstaclePatterns[i].weight;
            if (r <= acc) return i;
        }

        return -1;
    }
}
