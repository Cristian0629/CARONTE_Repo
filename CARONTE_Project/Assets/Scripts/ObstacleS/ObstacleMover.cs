using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float obstacleStartSpeed = 3.8f;
    public float obstacleMaxSpeed = 8.2f;
    public float extraSpeed = 0f;

    void Update()
    {
        float diff = 0f;

        if (GameSpeed_BG.Instance != null)
            diff = GameSpeed_BG.Instance.Difficulty01;

        float speed = Mathf.Lerp(obstacleStartSpeed, obstacleMaxSpeed, diff) + extraSpeed;

        // DEBUG: mira si esto se ejecuta
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[ObstacleMover] {name} speed={speed:F2} diff={diff:F2}");

        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}
