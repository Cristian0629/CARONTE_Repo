using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawnerPatterns : MonoBehaviour
{
    [Header("References")]
    public GameObject obstaclePrefab;

    [Header("Special obstacle (slow up-down + slow left drift)")]
    public GameObject slowMovingObstaclePrefab;
    [Range(0f, 1f)] public float slowMovingChance = 0.08f;
    [Tooltip("Si es true, este obstáculo especial solo puede salir en spawns normales (no dentro de clusters).")]
    public bool slowMovingOnlyOutsideClusters = true;

    [Header("On-screen obstacle cap")]
    [Tooltip("Máximo número de obstáculos permitidos dentro del rango visible de la cámara.")]
    public int maxObstaclesInCamera = 4;

    [Tooltip("Tag que llevan TODOS los obstáculos que quieres contar (normales + especiales).")]
    public string obstacleTag = "Obstacle";

    [Tooltip("Padding en X para contar también un poquito fuera de cámara (0 = solo visible).")]
    public float cameraCountPaddingX = 0.0f;

    [Tooltip("Si estás al tope, cuánto esperamos antes de intentar spawnear otra vez.")]
    public float fullScreenRetryDelay = 0.12f;

    [Header("Speed scaling (final boost only)")]
    public float referenceScrollSpeed = 6f;

    [Header("Spawn density boost near max speed")]
    public float boostStartSpeed = 12f;
    public float maxScrollSpeed = 18f;
    public float maxSpawnBoost = 1.6f;
    public float boostExponent = 3f;

    [Header("Midgame density compensation")]
    [Range(0f, 1f)] public float midCompensation = 0.55f;
    public float midMaxFactor = 1.35f;

    [Tooltip("Mínimo tiempo entre spawns para evitar spam en el mismo frame.")]
    public float minTimeBetweenSpawns = 0.05f;

    [Header("Spawn X")]
    public float spawnX = 12f;

    [Header("Y Range (easy -> hard)")]
    public float easyMinY = -2.0f;
    public float easyMaxY = 2.0f;
    public float hardMinY = -3.6f;
    public float hardMaxY = 3.6f;

    [Header("Spawn Interval (easy -> hard)")]
    public float easyInterval = 1.35f;
    public float hardInterval = 0.75f;

    [Header("Single timing randomizer (easy -> hard)")]
    public float easySingleEarly = 0.35f;
    public float hardSingleEarly = 0.55f;
    public float easySingleLate = 0.75f;
    public float hardSingleLate = 1.25f;

    [Header("Cluster control (single bursts)")]
    [Range(0f, 1f)] public float clusterChanceEasy = 0.05f;
    [Range(0f, 1f)] public float clusterChanceHard = 0.12f;
    public int clusterMinCount = 2;
    public int clusterMaxCount = 5;
    public float clusterMinGap = 0.06f;
    public float clusterMaxGap = 0.14f;
    public float clusterCooldownEasy = 1.8f;
    public float clusterCooldownHard = 0.9f;

    [Header("Anti-wall spacing (prevents blocking lanes)")]
    public int recentYCount = 4;
    public float burstDeltaMultiplier = 1.15f;

    [Header("Gap control (easy -> hard)")]
    public float easyMinDeltaY = 1.4f;
    public float hardMinDeltaY = 0.6f;

    [Header("Circle pattern (4 obstacles)")]
    [Range(0f, 1f)] public float circleChanceEasy = 0.35f;
    [Range(0f, 1f)] public float circleChanceHard = 0.55f;
    public float circleRadiusYEasy = 1.1f;
    public float circleRadiusYHard = 0.8f;
    public float circleRadiusX = 1.0f;
    public float circleCooldownEasy = 2.2f;
    public float circleCooldownHard = 1.2f;

    [Header("Stair pattern")]
    public int stairMinSteps = 3;
    public int stairMaxSteps = 6;
    public float stairStepHeight = 0.7f;
    public float stairStepXSpacing = 1.1f;

    [Header("Pattern chances")]
    [Range(0f, 1f)] public float stairChanceEasy = 0.08f;
    [Range(0f, 1f)] public float stairChanceHard = 0.22f;

    [Header("Pattern safe zone")]
    [Tooltip("Margen extra en X (unidades) para que no spawnee nada pegado alrededor de un patrón.")]
    public float patternSafeMarginX = 2.0f;

    [Tooltip("Tiempo extra mínimo (segundos) de silencio después de un patrón (además del cálculo por ancho).")]
    public float patternExtraSilence = 0.10f;

    float timer;
    float lastY = 999f;

    float patternCooldown;
    public float stairCooldownEasy = 1.1f;
    public float stairCooldownHard = 0.4f;

    int clusterRemaining = 0;
    float clusterCooldownTimer = 0f;

    float circleCooldownTimer = 0f;

    readonly Queue<float> recentYs = new Queue<float>();

    void Update()
    {
        float d = (GameSpeed_BG.Instance != null) ? GameSpeed_BG.Instance.Difficulty01 : 0f;
        float scrollSpeed = (GameSpeed_BG.Instance != null) ? GameSpeed_BG.Instance.CurrentSpeed : referenceScrollSpeed;

        float factor = GetSpawnFactor(scrollSpeed);

        if (clusterCooldownTimer > 0f) clusterCooldownTimer -= Time.deltaTime;
        if (circleCooldownTimer > 0f) circleCooldownTimer -= Time.deltaTime;

        if (patternCooldown > 0f)
        {
            patternCooldown -= Time.deltaTime * factor;
            return;
        }

        timer -= Time.deltaTime * factor;

        if (timer <= 0f)
        {
            
            if (!CanSpawnMore(1))
            {
                
                clusterRemaining = 0;
                timer = Mathf.Max(minTimeBetweenSpawns, fullScreenRetryDelay);
                return;
            }

            SpawnWithPatterns(d, factor, scrollSpeed);
        }
    }

    bool CanSpawnMore(int amountToAdd)
    {
        if (maxObstaclesInCamera <= 0) return true; 
        int current = CountObstaclesInCamera();
        return (current + amountToAdd) <= maxObstaclesInCamera;
    }

    int CountObstaclesInCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return 0;

        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x - cameraCountPaddingX;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f)).x + cameraCountPaddingX;

        GameObject[] obs = GameObject.FindGameObjectsWithTag(obstacleTag);

        int count = 0;
        for (int i = 0; i < obs.Length; i++)
        {
            if (obs[i] == null) continue;
            float x = obs[i].transform.position.x;
            if (x >= left && x <= right)
                count++;
        }

        return count;
    }

    float GetSpawnFactor(float scrollSpeed)
    {
        float speedRatio = scrollSpeed / Mathf.Max(0.01f, referenceScrollSpeed);

        float midFactor = Mathf.Lerp(1f, speedRatio, midCompensation);
        midFactor = Mathf.Clamp(midFactor, 1f, midMaxFactor);

        float t = Mathf.InverseLerp(boostStartSpeed, maxScrollSpeed, scrollSpeed);
        t = Mathf.Clamp01(t);
        t = Mathf.Pow(t, boostExponent);

        float endBoost = Mathf.Lerp(1f, maxSpawnBoost * 0.90f, t);

        return midFactor * endBoost;
    }

    void ApplyPatternSafeCooldown(float patternWidthX, float scrollSpeed)
    {
        float speed = Mathf.Max(0.01f, scrollSpeed);
        float totalWidthX = patternWidthX + patternSafeMarginX * 2f;
        float time = (totalWidthX / speed) + patternExtraSilence;
        patternCooldown = Mathf.Max(patternCooldown, time);
    }

    void SpawnWithPatterns(float d, float factor, float scrollSpeed)
    {
        float stairChance = Mathf.Lerp(stairChanceEasy, stairChanceHard, d);

        
        if (Random.value < stairChance && CanSpawnMore(3))
        {
            SpawnStair(d, factor, scrollSpeed);
            return;
        }

        // --- CLUSTER / SINGLE ---
        if (clusterRemaining > 0)
        {
            
            if (!CanSpawnMore(1))
            {
                clusterRemaining = 0;
                timer = Mathf.Max(minTimeBetweenSpawns, fullScreenRetryDelay);
                return;
            }

            
            if (circleCooldownTimer <= 0f && clusterRemaining >= 3 && CanSpawnMore(4))
            {
                float circleChance = Mathf.Lerp(circleChanceEasy, circleChanceHard, d);
                if (Random.value < circleChance)
                {
                    SpawnCircle4(d, scrollSpeed);
                    clusterRemaining = 0;

                    float cd = Mathf.Lerp(circleCooldownEasy, circleCooldownHard, d);
                    circleCooldownTimer = cd;

                    float after = Random.Range(clusterMinGap, clusterMaxGap);
                    timer = Mathf.Max(minTimeBetweenSpawns, after / factor);

                    clusterCooldownTimer = Mathf.Lerp(clusterCooldownEasy, clusterCooldownHard, d);
                    return;
                }
            }

            SpawnSingle(d, isBurst: true);

            clusterRemaining--;

            float gap = Random.Range(clusterMinGap, clusterMaxGap);
            timer = Mathf.Max(minTimeBetweenSpawns, gap / factor);

            if (clusterRemaining == 0)
                clusterCooldownTimer = Mathf.Lerp(clusterCooldownEasy, clusterCooldownHard, d);

            return;
        }

        
        if (!CanSpawnMore(1))
        {
            timer = Mathf.Max(minTimeBetweenSpawns, fullScreenRetryDelay);
            return;
        }

        SpawnSingle(d, isBurst: false);

        if (clusterCooldownTimer <= 0f)
        {
            float clusterChance = Mathf.Lerp(clusterChanceEasy, clusterChanceHard, d);
            if (Random.value < clusterChance)
            {
                int count = Random.Range(clusterMinCount, clusterMaxCount + 1);

                
                int current = CountObstaclesInCamera();
                int room = Mathf.Max(0, maxObstaclesInCamera - current);
                count = Mathf.Clamp(count, 1, Mathf.Max(1, room));

                clusterRemaining = Mathf.Max(0, count - 1);

                float gap = Random.Range(clusterMinGap, clusterMaxGap);
                timer = Mathf.Max(minTimeBetweenSpawns, gap / factor);
                return;
            }
        }

        float baseInterval = Mathf.Lerp(easyInterval, hardInterval, d);
        float early = Mathf.Lerp(easySingleEarly, hardSingleEarly, d);
        float late = Mathf.Lerp(easySingleLate, hardSingleLate, d);

        float negativeChance = Mathf.Lerp(0.10f, 0.18f, d);
        float offset = (Random.value < negativeChance) ? Random.Range(-early, 0f) : Random.Range(0f, late);

        timer = Mathf.Max(minTimeBetweenSpawns, (baseInterval + offset) / factor);
    }

    void SpawnSingle(float d, bool isBurst)
    {
        float minY = Mathf.Lerp(easyMinY, hardMinY, d);
        float maxY = Mathf.Lerp(easyMaxY, hardMaxY, d);

        float minDeltaY = Mathf.Lerp(easyMinDeltaY, hardMinDeltaY, d);
        if (isBurst) minDeltaY *= burstDeltaMultiplier;

        float y = PickYWithRecentSpacing(minY, maxY, minDeltaY);

        lastY = y;
        EnqueueRecentY(y);

        GameObject prefabToSpawn = obstaclePrefab;

        if (slowMovingObstaclePrefab != null)
        {
            if (!slowMovingOnlyOutsideClusters || !isBurst)
            {
                if (Random.value < slowMovingChance)
                    prefabToSpawn = slowMovingObstaclePrefab;
            }
        }

        Instantiate(prefabToSpawn, new Vector3(spawnX, y, 0f), Quaternion.identity);
    }

    float PickYWithRecentSpacing(float minY, float maxY, float minDeltaY)
    {
        const int attempts = 10;

        for (int a = 0; a < attempts; a++)
        {
            float candidate = Random.Range(minY, maxY);

            bool ok = true;

            if (Mathf.Abs(candidate - lastY) < minDeltaY) ok = false;

            if (ok)
            {
                foreach (float ry in recentYs)
                {
                    if (Mathf.Abs(candidate - ry) < minDeltaY)
                    {
                        ok = false;
                        break;
                    }
                }
            }

            if (ok) return candidate;
        }

        float fallback = Random.Range(minY, maxY);
        if (Mathf.Abs(fallback - lastY) < minDeltaY)
            fallback = Mathf.Clamp(fallback + Mathf.Sign(Random.value - 0.5f) * minDeltaY, minY, maxY);

        return fallback;
    }

    void EnqueueRecentY(float y)
    {
        recentYs.Enqueue(y);
        while (recentYs.Count > Mathf.Max(0, recentYCount))
            recentYs.Dequeue();
    }

    void SpawnCircle4(float d, float scrollSpeed)
    {
        
        if (!CanSpawnMore(4))
        {
            timer = Mathf.Max(minTimeBetweenSpawns, fullScreenRetryDelay);
            return;
        }

        float minY = Mathf.Lerp(easyMinY, hardMinY, d);
        float maxY = Mathf.Lerp(easyMaxY, hardMaxY, d);

        float radiusY = Mathf.Lerp(circleRadiusYEasy, circleRadiusYHard, d);

        float centerX = spawnX + circleRadiusX;
        float centerY = Random.Range(minY + radiusY, maxY - radiusY);

        float invSqrt2 = 0.70710678f;

        Vector3[] pts =
        {
            new Vector3(centerX + circleRadiusX * invSqrt2, centerY + radiusY * invSqrt2, 0f),
            new Vector3(centerX - circleRadiusX * invSqrt2, centerY + radiusY * invSqrt2, 0f),
            new Vector3(centerX - circleRadiusX * invSqrt2, centerY - radiusY * invSqrt2, 0f),
            new Vector3(centerX + circleRadiusX * invSqrt2, centerY - radiusY * invSqrt2, 0f),
        };

        for (int i = 0; i < pts.Length; i++)
        {
            Instantiate(obstaclePrefab, pts[i], Quaternion.identity);
            EnqueueRecentY(pts[i].y);
        }

        lastY = centerY;

        ApplyPatternSafeCooldown(patternWidthX: 2f * circleRadiusX, scrollSpeed: scrollSpeed);
    }

    void SpawnStair(float d, float factor, float scrollSpeed)
    {
        
        int steps = Random.Range(stairMinSteps, stairMaxSteps + 1);

        
        int current = CountObstaclesInCamera();
        int room = Mathf.Max(0, maxObstaclesInCamera - current);
        steps = Mathf.Clamp(steps, 3, Mathf.Max(3, room));

        
        if (!CanSpawnMore(steps))
        {
            timer = Mathf.Max(minTimeBetweenSpawns, fullScreenRetryDelay);
            return;
        }

        float minY = Mathf.Lerp(easyMinY, hardMinY, d);
        float maxY = Mathf.Lerp(easyMaxY, hardMaxY, d);

        int dir = (Random.value < 0.5f) ? 1 : -1;

        float totalHeight = (steps - 1) * stairStepHeight;
        float baseMin = minY + (dir == -1 ? totalHeight : 0f);
        float baseMax = maxY - (dir == 1 ? totalHeight : 0f);
        float baseY = Random.Range(baseMin, baseMax);

        float xSpacing = Mathf.Lerp(stairStepXSpacing, stairStepXSpacing * 0.85f, d);

        for (int i = 0; i < steps; i++)
        {
            float y = baseY + dir * (i * stairStepHeight);
            float x = spawnX + i * xSpacing;

            Instantiate(obstaclePrefab, new Vector3(x, y, 0f), Quaternion.identity);
        }

        lastY = baseY + dir * ((steps - 1) * stairStepHeight);

        patternCooldown = Mathf.Max(patternCooldown, Mathf.Lerp(stairCooldownEasy, stairCooldownHard, d));

        float stairWidthX = (steps - 1) * xSpacing;
        ApplyPatternSafeCooldown(patternWidthX: stairWidthX, scrollSpeed: scrollSpeed);
    }
}
