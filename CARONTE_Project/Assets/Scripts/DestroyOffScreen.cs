using UnityEngine;

public class DestroyOffscreen : MonoBehaviour
{
    public float leftPadding = 2f;

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        float destroyX = bottomLeft.x - leftPadding;

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
