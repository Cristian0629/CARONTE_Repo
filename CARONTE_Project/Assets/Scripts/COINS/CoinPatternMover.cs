using UnityEngine;

public class CoinPatternMover : MonoBehaviour
{
    [Tooltip("Extra opcional por si quieres que algunos patrones vayan más rápido/lento")]
    public float extraSpeed = 0f;

    void Update()
    {
        float speed = (GameSpeed_BG.Instance != null) ? GameSpeed_BG.Instance.CurrentSpeed : 4f;
        transform.Translate(Vector3.left * (speed + extraSpeed) * Time.deltaTime);
    }
}
