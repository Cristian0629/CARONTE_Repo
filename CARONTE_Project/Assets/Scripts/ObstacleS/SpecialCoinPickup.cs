using UnityEngine;

public class SpecialCoinPickup : MonoBehaviour
{
    bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        SpecialCoinManager.Instance?.Add(1);

        // desactiva collider para evitar “pickup fantasma”
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }
}
