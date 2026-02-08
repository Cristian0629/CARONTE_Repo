using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;

    // ✅ NUEVO: marcar si esta moneda es especial
    [SerializeField] private bool isSpecialCoin = false;

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        if (Currency.Instance != null)
        {
            if (isSpecialCoin)
                Currency.Instance.AddSpecialCoins(value); // 💜 especiales
            else
                Currency.Instance.AddCoins(value);        // 🟡 normales
        }

        // Desactiva el collider inmediatamente
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Ocultar sprite al instante
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Destroy(gameObject);
    }
}

