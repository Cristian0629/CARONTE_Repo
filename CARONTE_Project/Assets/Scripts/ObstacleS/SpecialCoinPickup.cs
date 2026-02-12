using UnityEngine;

public class SpecialCoinPickup : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip specialCoinSfx;
    [Range(0f, 1f)]
    [SerializeField] private float specialCoinSfxVolume = 0.45f;

    bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        SpecialCoinManager.Instance?.Add(1);
        Debug.Log("[SpecialCoinPickup] Added 1 to SpecialCoinManager");

        Currency.Instance?.AddSpecialCoins(1);
        Debug.Log("[SpecialCoinPickup] Added 1 to Currency.SpecialCoins");

        // ✅ SFX al recoger
        if (specialCoinSfx != null)
            AudioSource.PlayClipAtPoint(specialCoinSfx, transform.position, specialCoinSfxVolume);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }
}
