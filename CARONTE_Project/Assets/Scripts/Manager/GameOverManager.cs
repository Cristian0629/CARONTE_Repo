using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup gameOverGroup;

    [Header("Death SFX")]
    [SerializeField] private AudioClip deathSfx;
    [Range(0f, 1f)]
    [SerializeField] private float deathSfxVolume = 0.85f;

    private AudioSource deathSfxSource;

    [Header("Game Over Stats (TMP)")]
    [SerializeField] private TMP_Text metersResultText;
    [SerializeField] private TMP_Text coinsResultText;
    [SerializeField] private TMP_Text specialCoinsResultText;

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

    [Header("Extra Life - SlowMo on revive (como Pause)")]
    [SerializeField] private bool doReviveSlowMo = true;
    [SerializeField] private float reviveSlowMoScale = 0.25f;
    [SerializeField] private float reviveSlowMoDurationRealtime = 3f;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool shown;
    private bool isReviving;

    private Coroutine reviveSlowMoRoutine;

    public bool IsGameOverShown => shown;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Time.timeScale = 1f;

        
        deathSfxSource = gameObject.AddComponent<AudioSource>();
        deathSfxSource.playOnAwake = false;
        deathSfxSource.loop = false;
        deathSfxSource.spatialBlend = 0f;

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

        
        PlayDeathSfx();

        
        if (SceneMusicSwitcher.Instance != null)
            SceneMusicSwitcher.Instance.StopMusic();

        
        UpdateGameOverStatsUI();

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

    private void PlayDeathSfx()
    {
        if (deathSfx == null) return;

        if (deathSfxSource != null)
        {
            
            deathSfxSource.Stop();
            deathSfxSource.pitch = 1f;
            deathSfxSource.PlayOneShot(deathSfx, deathSfxVolume);
        }
    }

    private void UpdateGameOverStatsUI()
    {
        
        if (metersResultText != null)
        {
            float meters = 0f;
            if (UIHUD.Instance != null)
                meters = UIHUD.Instance.CurrentMeters;

            metersResultText.text = $"{meters:0} m";
        }

        
        if (coinsResultText != null)
        {
            int coins = (Currency.Instance != null) ? Currency.Instance.Coins : 0;
            coinsResultText.text = coins.ToString();
        }

        
        if (specialCoinsResultText != null)
        {
            if (SpecialCoinManager.Instance != null)
            {
                int current = SpecialCoinManager.Instance.Current;
                int target = SpecialCoinManager.Instance.target;
                specialCoinsResultText.text = $"{current}/{target}";

                if (debugLogs)
                    Debug.Log($"[GameOverManager] SpecialCoinManager Current={current}/{target}");
            }
            else
            {
                int sc = (Currency.Instance != null) ? Currency.Instance.SpecialCoins : 0;
                specialCoinsResultText.text = sc.ToString();

                if (debugLogs)
                    Debug.Log("[GameOverManager] SpecialCoinManager NULL, Currency.SpecialCoins=" + sc);
            }
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

        if (Currency.Instance == null) return;
        if (!Currency.Instance.TrySpendCoins(250)) return;

        isReviving = true;
        StartCoroutine(ExtraLifeRoutine());
    }

    private IEnumerator ExtraLifeRoutine()
    {
        if (debugLogs) Debug.Log("[GameOverManager] EXTRA LIFE");

        
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        Hide();

        ClearNearbyObstacles();

        yield return null;

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

        
        if (SceneMusicSwitcher.Instance != null)
            SceneMusicSwitcher.Instance.ResumeMusicWithRamp();

        
        if (doReviveSlowMo)
        {
            if (reviveSlowMoRoutine != null) StopCoroutine(reviveSlowMoRoutine);
            reviveSlowMoRoutine = StartCoroutine(ReviveSlowMoSequence());
        }

        shown = false;
        isReviving = false;
        yield return null;
    }

    private IEnumerator ReviveSlowMoSequence()
    {
        float startScale = reviveSlowMoScale;
        float endScale = 1f;

        Time.timeScale = startScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        float t = 0f;

        while (t < reviveSlowMoDurationRealtime)
        {
            t += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(t / reviveSlowMoDurationRealtime);
            float eased = Mathf.SmoothStep(0f, 1f, progress);

            Time.timeScale = Mathf.Lerp(startScale, endScale, eased);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        reviveSlowMoRoutine = null;
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
        {
            if (o.name.Contains("(Clone)"))
                Destroy(o);
        }
    }
}
