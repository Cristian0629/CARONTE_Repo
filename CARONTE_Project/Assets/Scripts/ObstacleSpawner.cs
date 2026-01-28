using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;

    [Header("Timing")]
    public float minSpawnTime = 0.8f;
    public float maxSpawnTime = 1.4f;

    [Header("Camera margins")]
    public float rightPadding = 1.5f;
    public float topPadding = 0.8f;
    public float bottomPadding = 0.8f;

    private float nextSpawn;

    void Start()
    {
        ScheduleNext();
    }

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
        nextSpawn = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Spawn()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("ObstacleSpawner: No hay prefabs asignados.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("ObstacleSpawner: No hay MainCamera con tag MainCamera.");
            return;
        }

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float spawnX = tr.x + rightPadding;
        float y = Random.Range(bl.y + bottomPadding, tr.y - topPadding);

        int idx = Random.Range(0, obstaclePrefabs.Length);
        Instantiate(obstaclePrefabs[idx], new Vector3(spawnX, y, 0f), Quaternion.identity);
    }
}
