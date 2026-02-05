using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Speed relative to background")]
    [Tooltip("Multiplicador al inicio del juego")]
    public float startSpeedMultiplier = 0.65f;

    [Tooltip("Multiplicador máximo respecto al fondo")]
    public float maxSpeedMultiplier = 0.9f;

    public float extraSpeed = 0f;

    void Update()
    {
        if (GameSpeed_BG.Instance == null) return;

        float diff = GameSpeed_BG.Instance.Difficulty01; // 0 → 1
        float bgSpeed = GameSpeed_BG.Instance.CurrentSpeed;

        // Interpolamos multiplicador según dificultad
        float multiplier = Mathf.Lerp(startSpeedMultiplier, maxSpeedMultiplier, diff);

        float finalSpeed = bgSpeed * multiplier + extraSpeed;

        transform.position += Vector3.left * finalSpeed * Time.deltaTime;
    }
}
