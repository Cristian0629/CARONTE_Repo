using UnityEngine;

public class SlowOscillatingObstacle : MonoBehaviour
{
    [Header("Left drift (independent of background)")]
    [Tooltip("Velocidad horizontal propia del obstáculo (no depende del background).")]
    public float leftSpeed = 2.0f;

    [Tooltip("Multiplicador extra para ajustar rápido sin tocar leftSpeed.")]
    public float leftSpeedMultiplier = 1.0f;

    [Header("Vertical oscillation")]
    [Tooltip("Cuánto sube/baja. (No tocamos la dificultad vertical)")]
    public float amplitude = 1.2f;

    [Tooltip("Ciclos por segundo. (No tocamos la dificultad vertical)")]
    public float frequency = 0.25f;

    [Header("On-screen cap")]
    [Tooltip("Máximo número de SlowOscillatingObstacle permitidos a la vez dentro de la cámara.")]
    public int maxInCamera = 2;

    [Tooltip("Padding horizontal para contar un poco fuera de cámara (recomendado 0).")]
    public float cameraPaddingX = 0f;

    [Tooltip("Cada cuánto comprobar el límite (segundos).")]
    public float capCheckInterval = 0.25f;

    [Header("Kill settings")]
    [Tooltip("Tag del jugador para detectar muerte.")]
    public string playerTag = "Player";

    [Tooltip("Si el Player tiene un script con método Die() o Kill(), intentará llamarlo.")]
    public bool callDieMethodIfExists = true;

    [Header("Optional")]
    public bool destroyOffscreen = true;
    public float destroyX = -14f;

    float startY;
    float phase;

    float nextCapCheckTime;

    void Start()
    {
        startY = transform.position.y;
        phase = Random.Range(0f, Mathf.PI * 2f);

        // ✅ Al aparecer, si ya hay 2 en cámara, este se destruye
        if (maxInCamera > 0 && CountSameTypeInCamera() >= maxInCamera)
        {
            Destroy(gameObject);
            return;
        }

        nextCapCheckTime = Time.time + capCheckInterval;
    }

    void Update()
    {
        // 1) Mover a la izquierda a velocidad propia (más rápido horizontalmente)
        float speed = leftSpeed * leftSpeedMultiplier;
        transform.position += Vector3.left * (speed * Time.deltaTime);

        // 2) Oscilar arriba/abajo (sin aumentar la dificultad vertical)
        float y = startY + Mathf.Sin((Time.time * frequency * Mathf.PI * 2f) + phase) * amplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        // 3) Destruir al salir
        if (destroyOffscreen && transform.position.x < destroyX)
        {
            Destroy(gameObject);
            return;
        }

        // ✅ Comprobación periódica del cap: si sobran, este se auto-destruye
        if (maxInCamera > 0 && Time.time >= nextCapCheckTime)
        {
            nextCapCheckTime = Time.time + capCheckInterval;

            if (CountSameTypeInCamera() > maxInCamera)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    int CountSameTypeInCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return 0;

        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x - cameraPaddingX;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f)).x + cameraPaddingX;

#if UNITY_2023_1_OR_NEWER
        var all = FindObjectsByType<SlowOscillatingObstacle>(FindObjectsSortMode.None);
#else
        var all = FindObjectsOfType<SlowOscillatingObstacle>();
#endif

        int count = 0;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            float x = all[i].transform.position.x;
            if (x >= left && x <= right)
                count++;
        }

        return count;
    }

    // --- Matar al jugador igual que un obstáculo normal ---
    void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.gameObject);
    }

    void TryKill(GameObject other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!callDieMethodIfExists) return;

        var comps = other.GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null) continue;

            var type = comps[i].GetType();
            var die = type.GetMethod("Die");
            if (die != null)
            {
                die.Invoke(comps[i], null);
                return;
            }

            var kill = type.GetMethod("Kill");
            if (kill != null)
            {
                kill.Invoke(comps[i], null);
                return;
            }
        }
    }
}
