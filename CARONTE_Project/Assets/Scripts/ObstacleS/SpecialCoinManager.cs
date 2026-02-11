using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[DefaultExecutionOrder(10000)]
public class SpecialCoinManager : MonoBehaviour
{
    public static SpecialCoinManager Instance { get; private set; }

    [Header("Win Condition")]
    public int target = 15;

    [Header("HUD")]
    public TMP_Text specialCoinsText;
    public string specialCoinsTextName = "SpecialCoinsText";

    [Header("Final Sequence")]
    [SerializeField] private string finalSceneName = "FinalDialogue";

    [Tooltip("Duración del efecto final (slow + fade out). Recomiendo 1.8 - 2.5")]
    [SerializeField] private float finalSequenceDuration = 2.0f;

    [Tooltip("TimeScale mínimo durante el final.")]
    [Range(0.01f, 1f)]
    [SerializeField] private float minTimeScale = 0.15f;

    [Tooltip("Fade a negro antes de cambiar de escena (usa el mismo que finalSequenceDuration).")]
    [SerializeField] private float fadeOutTime = 2.0f;

    [Tooltip("Fade desde negro en la escena FinalDialogue.")]
    [SerializeField] private float fadeInTime = 1.2f;

    public int Current { get; private set; }

    private bool endingSequenceStarted = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Current = 0;

        FindHUDIfNeeded();
        ForceHUD();
    }

    void Update()
    {
        if (specialCoinsText == null)
            FindHUDIfNeeded();
    }

    void LateUpdate()
    {
        ForceHUD();
    }

    public void Add(int amount)
    {
        if (endingSequenceStarted) return;
        if (Current >= target) return;

        Current = Mathf.Clamp(Current + amount, 0, target);
        ForceHUD();

        if (Current >= target)
        {
            Debug.Log($"✅ FINAL TRIGGER: Special Coins {Current}/{target}");
            StartCoroutine(FinalSequence_SlowAndFade());
        }
    }

    private IEnumerator FinalSequence_SlowAndFade()
    {
        endingSequenceStarted = true;

        // ✅ Por si el usuario tenía el tiempo tocado por otra cosa
        float startScale = Time.timeScale;

        // ✅ Empieza el FADE INMEDIATO (mientras hacemos slow motion)
        if (SceneFader.Instance != null)
        {
            // Asegura que el fade-out dure lo que tú quieres (y sea visible)
            SceneFader.Instance.FadeToScene(finalSceneName, fadeOutTime, fadeInTime);
        }
        else
        {
            Debug.LogWarning("⚠️ SceneFader.Instance no encontrado. Cargando FinalDialogue sin fade.");
        }

        // ✅ Empieza el SLOW INMEDIATO (sin fixedDeltaTime para evitar sensación rara)
        // Pequeño “tirón” inicial para que se note al instante:
        Time.timeScale = Mathf.Lerp(startScale, minTimeScale, 0.15f);

        float duration = Mathf.Max(0.01f, finalSequenceDuration);
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            // Suaviza mucho la sensación (0->1 suave)
            float smooth = p * p * (3f - 2f * p); // SmoothStep

            Time.timeScale = Mathf.Lerp(startScale, minTimeScale, smooth);
            yield return null;
        }

        Time.timeScale = minTimeScale;

        // Si por algún motivo no hay SceneFader, hacemos fallback con un delay y cambiamos escena
        if (SceneFader.Instance == null)
        {
            yield return new WaitForSecondsRealtime(fadeOutTime);
            Time.timeScale = 1f;
            SceneManager.LoadScene(finalSceneName);
        }

        // Importante: NO restauramos aquí el timeScale si usamos SceneFader,
        // porque al cargar escena tu SceneFader ya hace Time.timeScale = 1f en OnSceneLoaded.
    }

    void ForceHUD()
    {
        if (specialCoinsText != null)
            specialCoinsText.text = $"{Current}/{target}";
    }

    void FindHUDIfNeeded()
    {
        if (specialCoinsText != null) return;

        GameObject go = GameObject.Find(specialCoinsTextName);
        if (go != null)
            specialCoinsText = go.GetComponent<TMP_Text>();
    }
}
