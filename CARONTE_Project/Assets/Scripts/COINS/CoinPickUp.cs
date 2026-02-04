using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        Currency.Instance?.AddCoins(value);

        // Desactiva el collider inmediatamente
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Opcional: ocultar sprite al instante
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // Destruir al final del frame
        Destroy(gameObject);
    }
}
