using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueIntro : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    [Header("Typewriter")]
    [SerializeField] private float letterDelay = 0.03f;

    [Header("Next Scene")]
    [SerializeField] private string gameplaySceneName = "CARONTE_Scene";

    [Header("Fade To Black")]
    [SerializeField] private CanvasGroup fadeCanvasGroup; // arrastra aquí el CanvasGroup del FadePanel
    [SerializeField] private float fadeOutTime = 0.8f;

    private int index = 0;
    private Coroutine typingRoutine;
    private bool isTyping;
    private bool isEnding; // para bloquear input mientras hace fade

    void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        dialogueText.text = "";
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
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];
            yield return new WaitForSeconds(letterDelay);
        }

        isTyping = false;
        typingRoutine = null;
    }

    void FinishTypingInstant()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        dialogueText.text = lines[index];
        isTyping = false;
        typingRoutine = null;
    }

    void Next()
    {
        index++;

        // ✅ Si no quedan líneas, hacemos fade y cargamos escena
        if (index >= lines.Length)
        {
            StartCoroutine(FadeAndLoad());
            return;
        }

        ShowLine();
    }

    private IEnumerator FadeAndLoad()
    {
        isEnding = true;

        // Si no tienes fadeCanvasGroup asignado, carga directo
        if (fadeCanvasGroup == null)
        {
            SceneManager.LoadScene(gameplaySceneName);
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;

        float t = 0f;
        float start = fadeCanvasGroup.alpha;
        float end = 1f;

        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutTime);
            fadeCanvasGroup.alpha = Mathf.Lerp(start, end, p);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        SceneManager.LoadScene(gameplaySceneName);
    }
}
