using UnityEngine;

public class GameSessionStart : MonoBehaviour
{
    void Start()
    {
        Currency.Instance?.ResetAll();

        
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeFromBlack(0.25f);
    }
}
