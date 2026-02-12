using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Typewriter")]
    [SerializeField] private float charDelay = 0.03f;
    [SerializeField] private KeyCode advanceKey = KeyCode.E;

    [Header("Auto start (intro)")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private DialogueLine[] introLines;

    [Header("Player lock")]
    [SerializeField] private PlayerWaveRide playerController;

    [Header("Start Freeze Overlay")]
    [Tooltip("CanvasGroup de una Image negra a pantalla completa. Alpha 1 = negro, 0 = transparente.")]
    [SerializeField] private CanvasGroup blackFade;
    [SerializeField] private float fadeDuration = 1f;

    private DialogueLine[] lines;
    private int index;
    private Coroutine typingCoroutine;
    private bool isTyping;

    void Awake()
    {
        dialoguePanel.SetActive(false);

       
        if (blackFade != null)
        {
            blackFade.alpha = 1f;
            blackFade.blocksRaycasts = true;
            blackFade.interactable = true;
        }
    }

    void Start()
    {
        if (playOnStart && introLines != null && introLines.Length > 0)
        {
            StartDialogue(introLines);
        }
        else
        {
            
            Time.timeScale = 1f;
            if (blackFade != null) blackFade.alpha = 0f;
        }
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        if (Input.GetKeyDown(advanceKey))
        {
            if (isTyping) FinishTyping();
            else NextLine();
        }
    }

    public void StartDialogue(DialogueLine[] newLines)
    {
        lines = newLines;
        index = 0;

        
        Time.timeScale = 0f;

        
        if (playerController != null)
            playerController.enabled = false;

        dialoguePanel.SetActive(true);

        
        if (blackFade != null)
            StartCoroutine(FadeBlack(1f, 0f, fadeDuration));

        ShowLine(lines[index]);
    }

    private void ShowLine(DialogueLine line)
    {
        nameText.text = line.speakerName;

        portraitImage.enabled = line.portrait != null;
        portraitImage.sprite = line.portrait;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        bodyText.text = "";

        foreach (char c in text)
        {
            bodyText.text += c;
            
            yield return new WaitForSecondsRealtime(charDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishTyping()
    {
        if (!isTyping) return;

        isTyping = false;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = null;

        bodyText.text = lines[index].text;
    }

    private void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine(lines[index]);
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        typingCoroutine = null;
        isTyping = false;

        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        
        if (blackFade != null)
            yield return StartCoroutine(FadeBlack(0f, 1f, fadeDuration));

        
        dialoguePanel.SetActive(false);
        bodyText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;

        
        Time.timeScale = 1f;

        if (playerController != null)
            playerController.enabled = true;

        
        if (blackFade != null)
            yield return StartCoroutine(FadeBlack(1f, 0f, fadeDuration));
    }

    private IEnumerator FadeBlack(float from, float to, float duration)
    {
        if (blackFade == null) yield break;

        blackFade.alpha = from;
        float t = 0f;

        
        blackFade.blocksRaycasts = true;
        blackFade.interactable = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            blackFade.alpha = Mathf.Lerp(from, to, t / Mathf.Max(0.0001f, duration));
            yield return null;
        }

        blackFade.alpha = to;

        
        if (Mathf.Approximately(to, 0f))
        {
            blackFade.blocksRaycasts = false;
            blackFade.interactable = false;
        }
    }
}
