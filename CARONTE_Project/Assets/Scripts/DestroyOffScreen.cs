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

        // Si el objeto tiene hijos (como patrones), comprobamos el hijo más a la derecha
        float rightmostX = transform.position.x;

        foreach (Transform child in transform)
        {
            if (child.position.x > rightmostX)
                rightmostX = child.position.x;
        }

        if (rightmostX < destroyX)
            Destroy(gameObject);
    }
}
