using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [SerializeField] private string obstacleTag = "Obstacle";

    private bool dead;

    
    public void ResetDeath()
    {
        dead = false;
    }

    private void Die()
    {
        if (dead) return;
        dead = true;

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.GameOver();
        else
            Debug.LogWarning("No existe GameOverManager en la escena.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(obstacleTag))
            Die();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(obstacleTag))
            Die();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(obstacleTag))
            Die();
    }
}
