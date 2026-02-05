using UnityEngine;

public class RiverHandObstacle : MonoBehaviour
{
    [Header("Horizontal (same as background)")]
    public float extraSpeed = 0f;

    [Header("Vertical motion (from river bottom)")]
    [Tooltip("Altura base (Y) del río/suelo desde donde 'sale' la mano.")]
    public float baseY = -2.8f;

    [Tooltip("Cuánto sube y baja (amplitud).")]
    public float amplitude = 0.9f;

    [Tooltip("Ciclos por segundo (0.25 = lento).")]
    public float frequency = 0.35f;

    [Header("Optional: small grab lunge near player")]
    public bool lungeTowardsPlayer = true;
    public float lungeDistance = 0.35f;
    public float lungeRangeX = 2.0f;
    public string playerTag = "Player";

    [Header("Kill settings")]
    [Tooltip("Si el Player tiene un script con método Die() o Kill(), intentará llamarlo.")]
    public bool callDieMethodIfExists = true;

    float phase;
    Transform player;

    void Start()
    {
        phase = Random.Range(0f, Mathf.PI * 2f);

        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;

        var pos = transform.position;
        pos.y = baseY;
        transform.position = pos;
    }

    void Update()
    {
        float bgSpeed = (GameSpeed_BG.Instance != null) ? GameSpeed_BG.Instance.CurrentSpeed : 4f;
        transform.position += Vector3.left * (bgSpeed + extraSpeed) * Time.deltaTime;

        float y = baseY + Mathf.Sin((Time.time * frequency * Mathf.PI * 2f) + phase) * amplitude;

        if (lungeTowardsPlayer && player != null)
        {
            float dx = Mathf.Abs(player.position.x - transform.position.x);
            if (dx <= lungeRangeX)
            {
                float t = 1f - (dx / Mathf.Max(0.0001f, lungeRangeX));
                y += lungeDistance * t;
            }
        }

        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    // ✅ Igual que los otros obstáculos
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
