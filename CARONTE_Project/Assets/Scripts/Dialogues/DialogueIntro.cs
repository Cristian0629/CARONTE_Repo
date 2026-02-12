using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class DialogueIntro : MonoBehaviour
{
    public enum Speaker { Caronte, Protagonista }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea(2, 4)] public string text;
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

    [Header("Talk SFX (sonido por letra)")]
    [SerializeField] private AudioClip talkClip;

    [Tooltip("Volumen del bip (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float talkSfxVolume = 0.15f;

    [Tooltip("Máxima duración que se deja sonar por letra (segundos). Si tu clip es largo (5s), pon 0.04–0.08.")]
    [SerializeField] private float maxTalkSfxDuration = 0.06f;

    [Tooltip("Si true, no suena en espacios/saltos de línea.")]
    [SerializeField] private bool skipWhitespace = true;

    [Tooltip("Si true, no suena en signos como . , ! ? : ; etc.")]
    [SerializeField] private bool skipPunctuation = true;

    [Header("Pitch por personaje")]
    [Tooltip("Pitch aleatorio por letra para Caronte (como ahora).")]
    [SerializeField] private Vector2 carontePitchRange = new Vector2(0.90f, 1.10f);

    [Tooltip("Pitch aleatorio por letra para Prota (un poco más agudo, sin ser molesto).")]
    [SerializeField] private Vector2 protaPitchRange = new Vector2(1.02f, 1.18f);

    [Tooltip("Evita demasiados sonidos por segundo. 0 = sin límite.")]
    [SerializeField] private float minSfxInterval = 0.02f;

    private float lastSfxTimeUnscaled = -999f;

    
    private AudioSource talkSfxSource;
    private Coroutine cutSfxRoutine;
    private int cutToken;

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

    private Color caronteBaseColorSprite = Color.white;
    private Color protaBaseColorSprite = Color.white;
    private Color caronteBaseColorUI = Color.white;
    private Color protaBaseColorUI = Color.white;

    void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        
        talkSfxSource = gameObject.AddComponent<AudioSource>();
        talkSfxSource.playOnAwake = false;
        talkSfxSource.loop = false;
        talkSfxSource.spatialBlend = 0f;

        if (caronteSprite != null) caronteBaseColorSprite = caronteSprite.color;
        if (protaSprite != null) protaBaseColorSprite = protaSprite.color;

        if (caronteUIImage != null) caronteBaseColorUI = caronteUIImage.color;
        if (protaUIImage != null) protaBaseColorUI = protaUIImage.color;

        if (caronteBoxRoot != null) caronteBoxRoot.SetActive(false);
        if (protaBoxRoot != null) protaBoxRoot.SetActive(false);

        if (caronteDialogueText != null) caronteDialogueText.text = "";
        if (protaDialogueText != null) protaDialogueText.text = "";

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

        if (caronteBoxRoot != null) caronteBoxRoot.SetActive(caronteTalking);
        if (protaBoxRoot != null) protaBoxRoot.SetActive(!caronteTalking);

        currentTextTarget = caronteTalking ? caronteDialogueText : protaDialogueText;

        if (caronteDialogueText != null) caronteDialogueText.text = "";
        if (protaDialogueText != null) protaDialogueText.text = "";

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
            char c = line[i];

            currentTextTarget.text += c;

            TryPlayTalkSfx(c);

            yield return new WaitForSeconds(letterDelay);
        }

        isTyping = false;
        typingRoutine = null;
    }

    private void TryPlayTalkSfx(char c)
    {
        if (talkSfxSource == null) return;
        if (talkClip == null) return;

        if (skipWhitespace && char.IsWhiteSpace(c)) return;
        if (skipPunctuation && IsPunctuation(c)) return;

        if (minSfxInterval > 0f)
        {
            float now = Time.unscaledTime;
            if (now - lastSfxTimeUnscaled < minSfxInterval) return;
            lastSfxTimeUnscaled = now;
        }

        
        Vector2 range = (currentSpeaker == Speaker.Caronte) ? carontePitchRange : protaPitchRange;

        float minP = Mathf.Min(range.x, range.y);
        float maxP = Mathf.Max(range.x, range.y);
        talkSfxSource.pitch = Random.Range(minP, maxP);

        talkSfxSource.PlayOneShot(talkClip, talkSfxVolume);

        if (maxTalkSfxDuration > 0f)
        {
            cutToken++;
            if (cutSfxRoutine != null) StopCoroutine(cutSfxRoutine);
            cutSfxRoutine = StartCoroutine(CutSfxAfter(token: cutToken, seconds: maxTalkSfxDuration));
        }
    }

    private IEnumerator CutSfxAfter(int token, float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (token != cutToken) yield break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (token != cutToken) yield break;

        talkSfxSource.Stop();
        cutSfxRoutine = null;
    }

    private bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' ||
               c == ':' || c == ';' || c == '"' || c == '\'' ||
               c == '(' || c == ')' || c == '[' || c == ']' ||
               c == '-' || c == '—';
    }

    void FinishTypingInstant()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        if (currentTextTarget != null && index >= 0 && index < lines.Length)
            currentTextTarget.text = lines[index].text;

        isTyping = false;
        typingRoutine = null;

        if (talkSfxSource != null) talkSfxSource.Stop();
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

        if (caronteBoxRoot != null) caronteBoxRoot.SetActive(false);
        if (protaBoxRoot != null) protaBoxRoot.SetActive(false);

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

        if (thanksTextRoot != null) thanksTextRoot.SetActive(true);

        float appearDelay = 0.6f;
        float appearTime = 2.0f;
        float holdTime = 10.0f;
        float disappearTime = 3.5f;

        if (thanksCanvasGroup != null)
        {
            thanksCanvasGroup.alpha = 0f;

            yield return new WaitForSecondsRealtime(appearDelay);

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
            yield return new WaitForSecondsRealtime(appearDelay);
        }

        yield return new WaitForSecondsRealtime(holdTime);

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

        if (thanksTextRoot != null) thanksTextRoot.SetActive(false);

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
