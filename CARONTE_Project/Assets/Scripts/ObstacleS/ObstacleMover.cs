using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Tooltip("Velocidad extra para este obstáculo (opcional)")]
    public float extraSpeed = 0f;

    void Update()
    {
        float baseSpeed = (GameSpeed_BG.Instance != null)
            ? GameSpeed_BG.Instance.CurrentSpeed
            : 4f;

        transform.Translate(Vector3.left * (baseSpeed + extraSpeed) * Time.deltaTime);
    }
}
