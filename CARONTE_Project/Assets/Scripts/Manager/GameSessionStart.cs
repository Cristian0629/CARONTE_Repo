using UnityEngine;

public class GameSessionStart : MonoBehaviour
{
    void Start()
    {
        Currency.Instance?.ResetAll();
    }
}

