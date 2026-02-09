using UnityEngine;

public class MagnetHelmetPickup : MonoBehaviour
{
    [SerializeField] private float magnetDuration = 10f;

    private bool picked;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;
        if (!other.CompareTag("Player")) return;

        picked = true;

        var player = other.GetComponent<PlayerWaveRide>();
        if (player != null)
            player.ActivateMagnet(magnetDuration);

        // desactiva collider para evitar doble pickup
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // opcional: ocultar sprite al instante
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Destroy(gameObject);
    }
}
