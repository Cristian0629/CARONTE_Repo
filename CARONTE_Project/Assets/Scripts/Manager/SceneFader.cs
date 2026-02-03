using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Defaults")]
    [SerializeField] private float defaultFadeIn = 1.2f;
    [SerializeField] private float defaultFadeOut = 1.2f;

    [Header("Hold to start (only gameplay)")]
    [SerializeField] private string gameplaySceneName = "CARONTE_Scene";
    [SerializeField] private bool holdBlackUntilAnyInput = true;

    [Header("Startup fade (only first time in Main Menu)")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private float mainMenuStartupFadeIn = 2.5f;
    [SerializeField] private bool doMainMenuStartupFade = true;

    private bool didMainMenuStartupFade = false;
    private Coroutine currentRoutine;

    
    private float nextSceneFadeInTime = -1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        
        Time.timeScale = 1f;

        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        
        if (doMainMenuStartupFade && !didMainMenuStartupFade && scene.name == mainMenuSceneName)
        {
            didMainMenuStartupFade = true;
            currentRoutine = StartCoroutine(FadeIn(mainMenuStartupFadeIn));
            return;
        }

        
        if (holdBlackUntilAnyInput && scene.name == gameplaySceneName)
        {
            currentRoutine = StartCoroutine(HoldBlackThenFadeIn(defaultFadeIn));
            return;
        }

        
        float fadeInToUse = (nextSceneFadeInTime > 0f) ? nextSceneFadeInTime : defaultFadeIn;
        nextSceneFadeInTime = -1f; 

        currentRoutine = StartCoroutine(FadeIn(fadeInToUse));
    }

    private IEnumerator HoldBlackThenFadeIn(float fadeInTime)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Time.timeScale = 0f;

        while (!Input.anyKeyDown && Input.touchCount == 0 && !Input.GetMouseButtonDown(0))
            yield return null;

        Time.timeScale = 1f;
        yield return StartCoroutine(FadeIn(fadeInTime));
    }

    public void FadeToScene(string sceneName, float fadeOutTime, float fadeInTime)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeAndSwitch(sceneName, fadeOutTime, fadeInTime));
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(mainMenuSceneName, 0.35f, 1.2f);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        var scene = SceneManager.GetActiveScene().name;

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(scene, 0.35f, 0.35f);
        else
            SceneManager.LoadScene(scene);
    }

    public void FadeAndQuit(float fadeOutTime)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeQuitRoutine(fadeOutTime));
    }

    private IEnumerator FadeQuitRoutine(float fadeOutTime)
    {
        yield return StartCoroutine(FadeOut(fadeOutTime));

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    
    private IEnumerator FadeAndSwitch(string sceneName, float fadeOutTime, float fadeInTime)
    {
        yield return StartCoroutine(FadeOut(fadeOutTime));

        
        nextSceneFadeInTime = fadeInTime;

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut(float duration)
    {
        float t = 0f;
        canvasGroup.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeIn(float duration)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
