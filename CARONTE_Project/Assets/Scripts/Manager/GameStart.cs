using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Start()
    {
        if (Currency.Instance != null)
            Currency.Instance.ResetAll();
    }
}
