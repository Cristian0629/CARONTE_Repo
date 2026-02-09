using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names (must match exactly)")]
    [SerializeField] private string introDialogueSceneName = "IntroDialogue"; // ⬅️ NUEVO
    [SerializeField] private string gameSceneName = "CARONTE_Scene";
    [SerializeField] private string extrasSceneName = "Extras";
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Fade Durations")]
    [SerializeField] private float playFadeOut = 1.2f;
    [SerializeField] private float playFadeIn = 1.2f;

    [SerializeField] private float extrasFadeOut = 0.7f;
    [SerializeField] private float extrasFadeIn = 0.7f;

    public void PlayGame()
    {
        // ⬇️ AHORA VA A LA ESCENA DE DIÁLOGO
        SceneFader.Instance.FadeToScene(introDialogueSceneName, playFadeOut, playFadeIn);
    }

    public void OpenExtras()
    {
        SceneFader.Instance.FadeToScene(extrasSceneName, extrasFadeOut, extrasFadeIn);
    }

    public void ReturnToMainMenu()
    {
        SceneFader.Instance.FadeToScene(mainMenuSceneName, extrasFadeOut, extrasFadeIn);
    }

    public void ExitGame()
    {
        SceneFader.Instance.FadeAndQuit(extrasFadeOut);
    }
}
