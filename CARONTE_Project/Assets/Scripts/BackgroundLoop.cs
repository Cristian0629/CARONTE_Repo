using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    public float width = 17.78f;

    void Update()
    {
        float speed = (GameSpeed.Instance != null) ? GameSpeed.Instance.CurrentSpeed : 4f;

        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x <= -width)
        {
            transform.position += new Vector3(width * 2f, 0f, 0f);
        }
    }
}

