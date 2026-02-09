using UnityEngine;

public class GameSessionStart : MonoBehaviour
{
    void Start()
    {
        Currency.Instance?.ResetAll();

        // ✅ Fade-in corto al entrar al gameplay
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeFromBlack(0.25f);
    }
}
