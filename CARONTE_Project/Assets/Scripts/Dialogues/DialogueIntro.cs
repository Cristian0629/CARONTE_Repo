using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class DialogueIntro : MonoBehaviour
{
    public enum Speaker
    {
        Caronte,
        Protagonista
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;

        [TextArea(2, 4)]
        public string text;
    }

    [Header("Text Boxes (uno por personaje)")]
    [SerializeField] private GameObject caronteBoxRoot;
    [SerializeField] private TMP_Text caronteDialogueText;

    [SerializeField] private GameObject protaBoxRoot;
    [SerializeField] private TMP_Text protaDialogueText;

    [Header("Personajes (oscurecer el que NO habla)")]
    [SerializeField] private SpriteRenderer caronteSprite;
    [SerializeField] private SpriteRenderer protaSprite;

    [SerializeField] private Image caronteUIImage;
    [SerializeField] private Image protaUIImage;

    [Range(0f, 1f)]
    [SerializeField] private float dimMultiplier = 0.45f;

    [Header("Dialogue Lines")]
    [SerializeField] private DialogueLine[] lines;

    [Header("Typewriter")]
    [SerializeField] private float letterDelay = 0.03f;

    [Header("Next Scene (si NO es final)")]
    [SerializeField] private string gameplaySceneName = "CARONTE_Scene";

    [Header("Fade To Black (local)")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutTime = 1.2f;

    [Header("Final Outro (solo en FinalDialogue)")]
    [SerializeField] private bool isFinalDialogueScene = false;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Tooltip("Arrastra aquí el objeto del texto THANKS FOR PLAYING (o su padre). Déjalo desactivado al inicio.")]
    [SerializeField] private GameObject thanksTextRoot;

    [Tooltip("Opcional: si el THANKS tiene CanvasGroup, lo hace aparecer suave.")]
    [SerializeField] private CanvasGroup thanksCanvasGroup;

    [Tooltip("Cuánto tiempo se queda el THANKS en pantalla (5-7 recomendado).")]
    [SerializeField] private float thanksShowTime = 6f;

    [Tooltip("Fade a negro más largo en el final.")]
    [SerializeField] private float finalFadeOutTime = 3.0f;

    private int index = 0;
    private Coroutine typingRoutine;
    private bool isTyping;
    private bool isEnding;

    private Speaker currentSpeaker;
    private TMP_Text currentTextTarget;

    // ✅ Colores base para que NO se acumulen oscuridades
    private Color caronteBaseColorSprite = Color.white;
    private Color protaBaseColorSprite = Color.white;
    private Color caronteBaseColorUI = Color.white;
    private Color protaBaseColorUI = Color.white;

    void Start()
    {
        // Fade panel listo
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // Guardar colores originales
        if (caronteSprite != null) caronteBaseColorSprite = caronteSprite.color;
        if (protaSprite != null) protaBaseColorSprite = protaSprite.color;

        if (caronteUIImage != null) caronteBaseColorUI = caronteUIImage.color;
        if (protaUIImage != null) protaBaseColorUI = protaUIImage.color;

        // Oculta ambos textboxes al inicio
        if (caronteBoxRoot != null) caronteBoxRoot.SetActive(false);
        if (protaBoxRoot != null) protaBoxRoot.SetActive(false);

        // Limpia textos
        if (caronteDialogueText != null) caronteDialogueText.text = "";
        if (protaDialogueText != null) protaDialogueText.text = "";

        // THANKS oculto al inicio
        if (thanksTextRoot != null) thanksTextRoot.SetActive(false);
        if (thanksCanvasGroup != null) thanksCanvasGroup.alpha = 0f;

        index = 0;
        ShowLine();
    }

    void Update()
    {
        if (isEnding) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            if (isTyping)
            {
                FinishTypingInstant();
                return;
            }

            Next();
        }
    }

    void ShowLine()
    {
        if (lines == null || lines.Length == 0) return;
        if (index < 0 || index >= lines.Length) return;

        ApplySpeaker(lines[index].speaker);

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index].text));
    }

    void ApplySpeaker(Speaker speaker)
    {
        currentSpeaker = speaker;
        bool caronteTalking = (speaker == Speaker.Caronte);

        // ✅ Activar SOLO el textbox del que habla
        if (caronteBoxRoot != null) caronteBoxRoot.SetActive(caronteTalking);
        if (protaBoxRoot != null) protaBoxRoot.SetActive(!caronteTalking);

        currentTextTarget = caronteTalking ? caronteDialogueText : protaDialogueText;

        // Limpia ambos antes de escribir
        if (caronteDialogueText != null) caronteDialogueText.text = "";
        if (protaDialogueText != null) protaDialogueText.text = "";

        // Iluminación personajes (sin acumulación)
        SetDimFromBase(caronteIsDim: !caronteTalking, protaIsDim: caronteTalking);
    }

    void SetDimFromBase(bool caronteIsDim, bool protaIsDim)
    {
        if (caronteSprite != null)
            caronteSprite.color = ApplyDimToBase(caronteBaseColorSprite, caronteIsDim);

        if (protaSprite != null)
            protaSprite.color = ApplyDimToBase(protaBaseColorSprite, protaIsDim);

        if (caronteUIImage != null)
            caronteUIImage.color = ApplyDimToBase(caronteBaseColorUI, caronteIsDim);

        if (protaUIImage != null)
            protaUIImage.color = ApplyDimToBase(protaBaseColorUI, protaIsDim);
    }

    Color ApplyDimToBase(Color baseColor, bool dim)
    {
        float m = dim ? dimMultiplier : 1f;
        return new Color(baseColor.r * m, baseColor.g * m, baseColor.b * m, baseColor.a);
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        if (currentTextTarget == null)
        {
            isTyping = false;
            yield break;
        }

        currentTextTarget.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            currentTextTarget.text += line[i];
            yield return new WaitForSeconds(letterDelay);
        }

        isTyping = false;
        typingRoutine = null;
    }

    void FinishTypingInstant()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        if (currentTextTarget != null && index >= 0 && index < lines.Length)
            currentTextTarget.text = lines[index].text;

        isTyping = false;
        typingRoutine = null;
    }

    void Next()
    {
        index++;

        if (index >= lines.Length)
        {
            if (isFinalDialogueScene)
                StartCoroutine(FinalThanksAndBackToMenu());
            else
                StartCoroutine(FadeAndLoad(gameplaySceneName, fadeOutTime));

            return;
        }

        ShowLine();
    }

    private IEnumerator FadeAndLoad(string sceneName, float duration)
    {
        isEnding = true;

        // Si no hay fade, carga directo
        if (fadeCanvasGroup == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;

        float t = 0f;
        float start = fadeCanvasGroup.alpha;
        float end = 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            fadeCanvasGroup.alpha = Mathf.Lerp(start, end, p);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FinalThanksAndBackToMenu()
    {
        isEnding = true;

        // Oculta cajas de diálogo para que no se cuelen
        if (caronteBoxRoot != null) caronteBoxRoot.SetActive(false);
        if (protaBoxRoot != null) protaBoxRoot.SetActive(false);

        // 1) Fade a negro inicial (el que ya tenías)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;

            float t = 0f;
            float duration = Mathf.Max(0.01f, finalFadeOutTime);

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, p);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        // 2) Aparecen los textos (THANKS + créditos) poco a poco
        if (thanksTextRoot != null) thanksTextRoot.SetActive(true);

        float appearDelay = 0.6f;     // ⬅️ espera un poquito antes de que empiecen a salir
        float appearTime = 2.0f;      // ⬅️ cuánto tardan en aparecer
        float holdTime = 10.0f;       // ⬅️ cuánto tiempo se quedan para leer
        float disappearTime = 3.5f;   // ⬅️ 3-4s para desaparecer antes de ir al menú

        if (thanksCanvasGroup != null)
        {
            thanksCanvasGroup.alpha = 0f;

            // pequeño delay antes de aparecer
            yield return new WaitForSecondsRealtime(appearDelay);

            // aparecer suave
            float a = 0f;
            while (a < appearTime)
            {
                a += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(a / appearTime);
                thanksCanvasGroup.alpha = Mathf.Lerp(0f, 1f, p);
                yield return null;
            }
            thanksCanvasGroup.alpha = 1f;
        }
        else
        {
            // si no hay CanvasGroup, al menos esperamos el delay
            yield return new WaitForSecondsRealtime(appearDelay);
        }

        // 3) Tiempo para leer
        yield return new WaitForSecondsRealtime(holdTime);

        // 4) “Segundo fade” (aquí ya estamos en negro, así que hacemos desaparecer textos 3–4s)
        if (thanksCanvasGroup != null)
        {
            float d = 0f;
            while (d < disappearTime)
            {
                d += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(d / disappearTime);
                thanksCanvasGroup.alpha = Mathf.Lerp(1f, 0f, p);
                yield return null;
            }
            thanksCanvasGroup.alpha = 0f;
        }

        // (opcional) desactivar el grupo
        if (thanksTextRoot != null) thanksTextRoot.SetActive(false);

        // 5) Cargar menú (seguimos en negro)
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}