using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [SerializeField] private string obstacleTag = "Obstacle";

    private bool dead;

    private void Die()
    {
        if (dead) return;
        dead = true;

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.GameOver();
        else
            Debug.LogWarning("No existe GameOverManager en la escena.");
    }

    // Si tus obstáculos tienen IsTrigger = true
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(obstacleTag))
            Die();
    }

    // Importante: por si estabas ya SOLAPADO al reanudar desde Time.timeScale = 0
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(obstacleTag))
            Die();
    }

    // Si tus obstáculos NO son trigger (colisión normal)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(obstacleTag))
            Die();
    }
}
