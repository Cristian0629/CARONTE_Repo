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

    private int index = 0;
    private Coroutine typingRoutine;
    private bool isTyping;

    void Start()
    {
        dialogueText.text = "";
        ShowLine();
    }

    void Update()
    {
        // "Cualquier tecla" + click ratón + toque móvil
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            // Si está escribiendo, al pulsar completa la línea al instante
            if (isTyping)
            {
                FinishTypingInstant();
                return;
            }

            // Si ya estaba completa, avanzamos
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

        // Si ya no quedan líneas → entrar al juego
        if (index >= lines.Length)
        {
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        ShowLine();
    }
}
