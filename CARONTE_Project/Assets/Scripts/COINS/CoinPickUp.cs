using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private bool isSpecialCoin = false;

    private bool collected = false;

    void Update()
    {
        var p = PlayerWaveRide.Instance;
        if (p == null) return;
        if (!p.MagnetActive) return;
        if (collected) return;

        // ✅ Target del imán (usa MagnetPoint si existe)
        Vector3 targetPos = (p.MagnetPoint != null) ? p.MagnetPoint.position : p.transform.position;

        float dist = Vector2.Distance(transform.position, targetPos);
        if (dist > p.MagnetRadius) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            p.MagnetPullSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        if (Currency.Instance != null)
        {
            if (isSpecialCoin) Currency.Instance.AddSpecialCoins(value);
            else Currency.Instance.AddCoins(value);
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Destroy(gameObject);
    }
}
