using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float defaultFadeIn = 1.2f;
    [SerializeField] private float defaultFadeOut = 1.2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cuando entra a cualquier escena → empieza negro y aparece
        StartCoroutine(FadeIn(defaultFadeIn));
    }

    public void FadeToScene(string sceneName, float fadeOutTime, float fadeInTime)
    {
        StartCoroutine(FadeAndSwitch(sceneName, fadeOutTime, fadeInTime));
    }

    public void FadeAndQuit(float fadeOutTime)
    {
        StartCoroutine(FadeQuitRoutine(fadeOutTime));
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
        SceneManager.LoadScene(sceneName);
        defaultFadeIn = fadeInTime;
    }

    private IEnumerator FadeOut(float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private IEnumerator FadeIn(float duration)
    {
        canvasGroup.alpha = 1;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}
