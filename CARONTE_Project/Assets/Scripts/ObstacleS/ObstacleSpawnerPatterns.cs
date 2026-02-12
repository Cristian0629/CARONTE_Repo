using System.Collections;
using UnityEngine;

public class ObstacleSpawnerPatterns : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;

    [Header("Timing")]
    public float minSpawnTime = 0.8f;
    public float maxSpawnTime = 1.4f;

    [Header("Camera margins")]
    public float rightPadding = 1.5f;
    public float topPadding = 0.8f;
    public float bottomPadding = 0.8f;

    [Header("Resume settings")]
    public float resumeDelay = 2f;          
    public string obstacleTag = "Obstacle"; 

    float nextSpawn;
    bool spawningEnabled = true;
    Coroutine resumeRoutine;

    void Start()
    {
        ScheduleNext();
    }

    void Update()
    {
        if (!spawningEnabled) return;

        nextSpawn -= Time.deltaTime;
        if (nextSpawn <= 0f)
        {
            Spawn();
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        nextSpawn = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Spawn()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float spawnX = tr.x + rightPadding;
        float y = Random.Range(bl.y + bottomPadding, tr.y - topPadding);

        int idx = Random.Range(0, obstaclePrefabs.Length);
        Instantiate(obstaclePrefabs[idx], new Vector3(spawnX, y, 0f), Quaternion.identity);
    }

    
    public void StopSpawningAndClear()
    {
        spawningEnabled = false;

        
        if (resumeRoutine != null)
        {
            StopCoroutine(resumeRoutine);
            resumeRoutine = null;
        }

        
        ClearObstacles();
    }

    
    public void ResumeSpawningWithDelay()
    {
        
        if (resumeRoutine != null) StopCoroutine(resumeRoutine);
        resumeRoutine = StartCoroutine(ResumeRoutine());
    }

    IEnumerator ResumeRoutine()
    {
        spawningEnabled = false;

        
        yield return new WaitForSecondsRealtime(resumeDelay);

        
        ScheduleNext();

        spawningEnabled = true;
        resumeRoutine = null;
    }

    void ClearObstacles()
    {
        
        GameObject[] obs = GameObject.FindGameObjectsWithTag(obstacleTag);
        for (int i = 0; i < obs.Length; i++)
            Destroy(obs[i]);
    }
}
