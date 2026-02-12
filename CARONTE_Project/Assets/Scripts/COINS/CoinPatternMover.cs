using UnityEngine;

public class CoinPatternMover : MonoBehaviour
{
    [Tooltip("Extra opcional por si quieres que algunos patrones vayan más rápido/lento")]
    public float extraSpeed = 0f;

    void Update()
    {
        if (GameSpeed_BG.Instance == null) return;

    
        float speed = GameSpeed_BG.Instance.CurrentSpeed;

        transform.Translate(Vector3.left * (speed + extraSpeed) * Time.deltaTime);
    }
}
