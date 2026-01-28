using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float extraSpeed = 0f; 

    void Update()
    {
        float speed = (GameSpeed.Instance != null) ? GameSpeed.Instance.CurrentSpeed : 4f;
        transform.Translate(Vector3.left * (speed + extraSpeed) * Time.deltaTime);
    }
}
