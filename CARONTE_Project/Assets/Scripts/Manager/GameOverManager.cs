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
    [SerializeField] private PlayerWaveRide playerController; 
    [SerializeField] private Rigidbody2D playerRb;            

   
    [SerializeField] private PlayerHit playerHit;

    [Header("Stop systems")]
    [SerializeField] private MonoBehaviour[] spawnersToStop; 

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

    [Header("Extra Lives (for coins later)")]
    [SerializeField] private int extraLivesAvailable = 1; 
    private bool isReviving; 


    
    public bool IsGameOverShown => shown;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        
        Time.timeScale = 1f;

        AutoSetupPlayerRefs();
        Hide();
    }

    private void AutoSetupPlayerRefs()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            if (playerController == null) playerController = player.GetComponent<PlayerWaveRide>();
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();

            
            if (playerHit == null) playerHit = player.GetComponent<PlayerHit>();
        }

        if (debugLogs)
        {
            Debug.Log($"[GameOverManager] player={(player ? player.name : "NULL")} " +
                      $"controller={(playerController ? "OK" : "NULL")} rb={(playerRb ? "OK" : "NULL")} " +
                      $"playerHit={(playerHit ? "OK" : "NULL")}");
        }
    }

    public void GameOver()
    {
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        if (shown) return;
        shown = true;

        if (debugLogs) Debug.Log("[GameOverManager] SHOW GAME OVER");

        
        var pause = FindFirstObjectByType<PauseMenu>();
        if (pause != null) pause.ForceClose();

        if (freezeWholeGame)
            Time.timeScale = 0f;

        if (spawnersToStop != null)
        {
            foreach (var s in spawnersToStop)
                if (s != null) s.enabled = false;
        }

        StopPlayerCompletely();

        if (gameOverGroup != null)
        {
            gameOverGroup.alpha = 1f;
            gameOverGroup.interactable = true;
            gameOverGroup.blocksRaycasts = true;

            if (debugLogs) Debug.Log("GAME OVER UI ACTIVADA");
        }
    }

    private void StopPlayerCompletely()
    {
        if (player == null || (playerController == null && playerRb == null))
            AutoSetupPlayerRefs();

        if (playerController != null)
            playerController.enabled = false;

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
        Time.timeScale = 0f;

        var scene = SceneManager.GetActiveScene().name;

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(scene, 0.6f, 0.3f);
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(scene);
        }
    }

    public void MainMenu()
    {
        Time.timeScale = 0f;

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(mainMenuSceneName, 1.0f, 0.5f);
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void ExtraLife()
    {
        if (!allowExtraLife) return;
        if (!shown) return;                 
        if (isReviving) return;             
        if (extraLivesAvailable <= 0) return;

        extraLivesAvailable--;
        isReviving = true;

        StartCoroutine(ExtraLifeRoutine());
    }


    private IEnumerator ExtraLifeRoutine()
    {
        if (debugLogs) Debug.Log("[GameOverManager] EXTRA LIFE");

        Time.timeScale = 1f;

        Hide();

        
        ClearNearbyObstacles();

        if (GameSpeed_BG.Instance != null)
        {
            GameSpeed_BG.Instance.ReduceSpeed(slowDownAmount);
            GameSpeed_BG.Instance.RewindDifficultySeconds(difficultyBackSeconds);
        }

        if (spawnersToStop != null)
        {
            foreach (var s in spawnersToStop)
                if (s != null) s.enabled = true;
        }

        ResumePlayerCompletely();

        
        if (playerHit == null) AutoSetupPlayerRefs();
        if (playerHit != null)
            playerHit.ResetDeath();

        shown = false;
        isReviving = false;
        yield return null;
    }

    public void AddExtraLife(int amount = 1)
    {
        extraLivesAvailable += Mathf.Max(0, amount);
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
        
        var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var o in obstacles)
            Destroy(o);
    }
}
