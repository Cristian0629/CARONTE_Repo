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

    private DialogueLine[] lines;
    private int index;
    private Coroutine typingCoroutine;
    private bool isTyping;

    void Awake()
    {
        dialoguePanel.SetActive(false);
    }

    void Start()
    {
        if (playOnStart && introLines != null && introLines.Length > 0)
        {
            StartDialogue(introLines);
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

        
        if (playerController != null)
            playerController.enabled = false;

        dialoguePanel.SetActive(true);
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
            yield return new WaitForSeconds(charDelay);
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

        dialoguePanel.SetActive(false);
        bodyText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;

        
        if (playerController != null)
            playerController.enabled = true;
    }
}
