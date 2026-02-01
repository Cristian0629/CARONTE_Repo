using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup gameOverGroup;

    [Header("Freeze whole game on Game Over")]
    [SerializeField] private bool freezeWholeGame = true;

    [Header("Player (auto si lo dejas vacío)")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerWaveRide playerController; // tu script de control del player (opcional)
    [SerializeField] private Rigidbody2D playerRb;            // opcional

    [Header("Stop systems")]
    [SerializeField] private MonoBehaviour[] spawnersToStop; // ObstacleSpawnerPatterns, coin spawner, etc.

    [Header("Extra Life")]
    [SerializeField] private bool allowExtraLife = true;
    [SerializeField] private float clearRadius = 10f;
    [SerializeField] private float slowDownAmount = 2.0f;
    [SerializeField] private float difficultyBackSeconds = 20f;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool shown;
    private bool usedExtraLife;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Seguridad por si vienes de una escena pausada
        Time.timeScale = 1f;

        AutoSetupPlayerRefs();
        Hide();
    }

    private void AutoSetupPlayerRefs()
    {
        // Si no está asignado, intenta encontrar por Tag "Player"
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            if (playerController == null) playerController = player.GetComponent<PlayerWaveRide>();
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();
        }

        if (debugLogs)
        {
            Debug.Log($"[GameOverManager] player={(player ? player.name : "NULL")} " +
                      $"controller={(playerController ? "OK" : "NULL")} rb={(playerRb ? "OK" : "NULL")}");
        }
    }

    // Llama esto desde tu detector de obstáculos (PlayerHit, etc.)
    public void GameOver()
    {
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        if (shown) return;
        shown = true;

        if (debugLogs) Debug.Log("[GameOverManager] SHOW GAME OVER");

        // Congelar TODO el juego
        if (freezeWholeGame)
            Time.timeScale = 0f;

        // parar spawners (opcional; si Time.timeScale=0 ya se paran solos, pero no molesta)
        if (spawnersToStop != null)
        {
            foreach (var s in spawnersToStop)
                if (s != null) s.enabled = false;
        }

        // parar player (si NO congelas el juego entero, esto es necesario)
        // si sí congelas todo, también lo dejamos por seguridad (no molesta)
        StopPlayerCompletely();

        // mostrar UI
        if (gameOverGroup != null)
        {
            gameOverGroup.alpha = 1f;
            gameOverGroup.interactable = true;
            gameOverGroup.blocksRaycasts = true;
        }
    }

    private void StopPlayerCompletely()
    {
        // Por si se instanció un player nuevo y los refs quedaron viejos
        if (player == null || (playerController == null && playerRb == null))
            AutoSetupPlayerRefs();

        // Desactivar control
        if (playerController != null)
            playerController.enabled = false;

        // Congelar físico completamente (por si freezeWholeGame está false o por seguridad)
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = false;
        }
    }

    private void ResumePlayerCompletely()
    {
        if (player == null || (playerController == null && playerRb == null))
            AutoSetupPlayerRefs();

        if (playerRb != null)
            playerRb.simulated = true;

        if (playerController != null)
            playerController.enabled = true;
    }

    public void Replay()
    {
        // Reanudar tiempo SIEMPRE antes de recargar
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExtraLife()
    {
        if (!allowExtraLife) return;
        if (usedExtraLife) return; // solo 1 uso
        usedExtraLife = true;

        StartCoroutine(ExtraLifeRoutine());
    }

    private IEnumerator ExtraLifeRoutine()
    {
        if (debugLogs) Debug.Log("[GameOverManager] EXTRA LIFE");

        // Reanudar el juego (si estaba congelado)
        Time.timeScale = 1f;

        Hide();

        // limpiar obstáculos cercanos
        ClearNearbyObstacles();

        // bajar velocidad + rebobinar un poco la dificultad/tiempo
        if (GameSpeed_BG.Instance != null)
        {
            GameSpeed_BG.Instance.ReduceSpeed(slowDownAmount);
            GameSpeed_BG.Instance.RewindDifficultySeconds(difficultyBackSeconds);
        }

        // reactivar spawners
        if (spawnersToStop != null)
        {
            foreach (var s in spawnersToStop)
                if (s != null) s.enabled = true;
        }

        // reactivar player
        ResumePlayerCompletely();

        shown = false;
        yield return null;
    }

    private void Hide()
    {
        if (gameOverGroup == null) return;
        gameOverGroup.alpha = 0f;
        gameOverGroup.interactable = false;
        gameOverGroup.blocksRaycasts = false;
    }

    private void ClearNearbyObstacles()
    {
        if (player == null) return;

        var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var o in obstacles)
        {
            float d = Vector2.Distance(o.transform.position, player.position);
            if (d <= clearRadius) Destroy(o);
        }
    }
}
