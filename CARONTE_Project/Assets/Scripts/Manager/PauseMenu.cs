using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Config")]
    [SerializeField] private float slowMoScale = 0.25f;
    [SerializeField] private float slowMoDurationRealtime = 3f;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private bool isPaused = false;
    private Coroutine resumeRoutine;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // valor normal de física
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) Pause();
            else ContinueWithSlowMo();
        }
    }

    public void Pause()
    {
        if (resumeRoutine != null)
        {
            StopCoroutine(resumeRoutine);
            resumeRoutine = null;
        }

        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ContinueWithSlowMo()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (resumeRoutine != null) StopCoroutine(resumeRoutine);
        resumeRoutine = StartCoroutine(ResumeSequence());
    }

    private IEnumerator ResumeSequence()
    {
        isPaused = false;

        float startScale = slowMoScale;
        float endScale = 1f;

        Time.timeScale = startScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        float t = 0f;

        while (t < slowMoDurationRealtime)
        {
            t += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(t / slowMoDurationRealtime);

            // Curva suave para que acelere progresivamente
            float eased = Mathf.SmoothStep(0f, 1f, progress);

            Time.timeScale = Mathf.Lerp(startScale, endScale, eased);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        resumeRoutine = null;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        var scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    public void MainMenu()
    {
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (pausePanel != null) pausePanel.SetActive(false);

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(mainMenuSceneName, 1.0f, 0.5f);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }
}
