using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PatternEntry
    {
        public GameObject prefab;     
        [Min(0f)] public float weight = 1f;
    }

    [Header("Obstacle patterns (prefabs)")]
    public PatternEntry[] obstaclePatterns;

    [Header("Timing (difficulty scaled)")]
    public float easyMinSpawn = 0.7f;
    public float easyMaxSpawn = 1.1f;

    public float hardMinSpawn = 0.25f;
    public float hardMaxSpawn = 0.45f;

    [Header("Burst spawn (optional 2nd obstacle)")]
    [Range(0f, 1f)] public float burstChance = 0.35f;
    public float burstDelayMin = 0.15f;
    public float burstDelayMax = 0.30f;

    [Tooltip("Si usas lanes, intentará forzar que el segundo spawn salga en una lane distinta.")]
    public bool burstForceDifferentLane = true;

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
            diff = GameSpeed_BG.Instance.Difficulty01; 

        
        float minT = Mathf.Lerp(easyMinSpawn, hardMinSpawn, diff);
        float maxT = Mathf.Lerp(easyMaxSpawn, hardMaxSpawn, diff);

        nextSpawn = Random.Range(minT, maxT);
    }

    void Spawn()
    {
        
        if (!SpawnSingle(null)) return;

        
        if (Random.value < burstChance)
        {
            float delay = Random.Range(burstDelayMin, burstDelayMax);
            StartCoroutine(SpawnBurst(delay));
        }
    }

    IEnumerator SpawnBurst(float delay)
    {
        yield return new WaitForSeconds(delay);

        
        float? avoidLane = null;
        if (useLanes && burstForceDifferentLane && obstacleLanesY != null && obstacleLanesY.Length > 1)
        {
            
        }

        SpawnSingle(avoidLane);
    }

    
    bool SpawnSingle(float? avoidLaneY)
    {
        if (obstaclePatterns == null || obstaclePatterns.Length == 0) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float spawnX = tr.x + rightPadding;

        float bottom = bl.y + bottomPadding;
        float top = tr.y - topPadding;

        float y = useLanes ? PickSafeLaneY(bottom, top, avoidLaneY) : Random.Range(bottom, top);

        int idx = PickWeightedIndexNoRepeat();
        if (idx < 0) return false;

        GameObject prefab = obstaclePatterns[idx].prefab;
        if (prefab == null) return false;

        GameObject obj = Instantiate(prefab, new Vector3(spawnX, y, 0f), Quaternion.identity);

        
        var mover = obj.GetComponent<ObstacleMover>();
        if (mover == null) mover = obj.AddComponent<ObstacleMover>();
        mover.extraSpeed = 0f;

        
        if (obj.GetComponent<DestroyOffscreen>() == null)
            obj.AddComponent<DestroyOffscreen>();

        lastIndex = idx;

        
        _lastSpawnY = y;

        return true;
    }

    float _lastSpawnY = float.NaN;

    float PickSafeLaneY(float bottom, float top, float? avoidLaneY)
    {
        if (obstacleLanesY == null || obstacleLanesY.Length == 0)
            return Random.Range(bottom, top);

        
        int safeCount = 0;
        for (int i = 0; i < obstacleLanesY.Length; i++)
        {
            float lane = obstacleLanesY[i];
            if (lane < bottom || lane > top) continue;

            
            if (avoidLaneY.HasValue && Mathf.Approximately(lane, avoidLaneY.Value)) continue;
            if (burstForceDifferentLane && !float.IsNaN(_lastSpawnY) && Mathf.Approximately(lane, _lastSpawnY))
            {
                
                continue;
            }

            safeCount++;
        }

        
        if (safeCount == 0)
        {
            
            int safeCount2 = 0;
            for (int i = 0; i < obstacleLanesY.Length; i++)
                if (obstacleLanesY[i] >= bottom && obstacleLanesY[i] <= top) safeCount2++;

            if (safeCount2 == 0)
                return Mathf.Clamp(obstacleLanesY[Random.Range(0, obstacleLanesY.Length)], bottom, top);

            int pick2 = Random.Range(0, safeCount2);
            for (int i = 0; i < obstacleLanesY.Length; i++)
            {
                if (obstacleLanesY[i] < bottom || obstacleLanesY[i] > top) continue;
                if (pick2-- == 0) return obstacleLanesY[i];
            }

            return 0f;
        }

        
        int pick = Random.Range(0, safeCount);
        for (int i = 0; i < obstacleLanesY.Length; i++)
        {
            float lane = obstacleLanesY[i];
            if (lane < bottom || lane > top) continue;

            if (avoidLaneY.HasValue && Mathf.Approximately(lane, avoidLaneY.Value)) continue;
            if (burstForceDifferentLane && !float.IsNaN(_lastSpawnY) && Mathf.Approximately(lane, _lastSpawnY)) continue;

            if (pick-- == 0) return lane;
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
