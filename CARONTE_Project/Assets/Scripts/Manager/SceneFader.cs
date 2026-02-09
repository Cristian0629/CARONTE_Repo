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

    // (-1 = usar defaultFadeIn)
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

        // por si vienes de pausas
        Time.timeScale = 1f;

        // empezar tapando
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        // 1) Fade lento SOLO al iniciar por primera vez en el main menu
        if (doMainMenuStartupFade && !didMainMenuStartupFade && scene.name == mainMenuSceneName)
        {
            didMainMenuStartupFade = true;
            currentRoutine = StartCoroutine(FadeIn(mainMenuStartupFadeIn));
            return;
        }

        // 2) ✅ Si venimos con FadeToScene, usamos ese fade-in y NO hacemos Hold.
        float fadeInToUse = (nextSceneFadeInTime > 0f) ? nextSceneFadeInTime : defaultFadeIn;
        bool cameFromFadeToScene = (nextSceneFadeInTime > 0f);
        nextSceneFadeInTime = -1f;

        // 3) Hold SOLO si es gameplay y NO venimos de FadeToScene
        if (holdBlackUntilAnyInput && scene.name == gameplaySceneName && !cameFromFadeToScene)
        {
            currentRoutine = StartCoroutine(HoldBlackThenFadeIn(fadeInToUse));
            return;
        }

        // 4) Resto: fade normal
        currentRoutine = StartCoroutine(FadeIn(fadeInToUse));
    }

    private IEnumerator HoldBlackThenFadeIn(float fadeInTime)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

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

    public void FadeFromBlack(float duration)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeIn(duration));
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

        // guardamos el fade-in para la siguiente escena
        nextSceneFadeInTime = fadeInTime;

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut(float duration)
    {
        if (canvasGroup == null) yield break;

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
        if (canvasGroup == null) yield break;

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
