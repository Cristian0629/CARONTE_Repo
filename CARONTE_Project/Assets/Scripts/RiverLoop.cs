using UnityEngine;

public class RiverLoop : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    Transform r1, r2;
    float width;

    void Start()
    {
        r1 = transform.GetChild(0);
        r2 = transform.GetChild(1);

        width = r1.GetComponent<SpriteRenderer>().bounds.size.x;


        r1.localPosition = Vector3.zero;
        r2.localPosition = new Vector3(width, 0f, 0f);
    }

    void Update()
    {
        float move = speed * Time.deltaTime;
        r1.position += Vector3.left * move;
        r2.position += Vector3.left * move;

        float camLeft = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect;

        
        if (r1.position.x + width < camLeft)
            r1.position = new Vector3(r2.position.x + width, r1.position.y, r1.position.z);

        if (r2.position.x + width < camLeft)
            r2.position = new Vector3(r1.position.x + width, r2.position.y, r2.position.z);
    }
}
