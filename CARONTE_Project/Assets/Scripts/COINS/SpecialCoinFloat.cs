using UnityEngine;

public class SpecialCoinFloat : MonoBehaviour
{
    [Header("Clamp in screen")]
    public float minY = -2.2f;
    public float maxY = 2.8f;

    float baseY;
    float targetY;
    float timer;

    void Start()
    {
        baseY = transform.position.y;
        PickNewTarget();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            PickNewTarget();

        // Parámetros por fases
        int sc = (Currency.Instance != null) ? Currency.Instance.SpecialCoins : 0;

        float amplitude;
        float yChangeSpeed;
        float tMin, tMax;

        if (sc < 15)
        {
            amplitude = 1.8f;
            yChangeSpeed = 2.0f;
            tMin = 0.35f; tMax = 0.85f;
        }
        else if (sc < 35)
        {
            amplitude = 2.2f;
            yChangeSpeed = 2.3f;
            tMin = 0.25f; tMax = 0.7f;
        }
        else
        {
            amplitude = 2.6f;
            yChangeSpeed = 2.6f;
            tMin = 0.20f; tMax = 0.55f;
        }

        float y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * yChangeSpeed);
        y = Mathf.Clamp(y, minY, maxY);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        // Guardamos parámetros actuales para el siguiente target
        _amplitude = amplitude;
        _tMin = tMin;
        _tMax = tMax;
    }

    float _amplitude = 2f;
    float _tMin = 0.3f;
    float _tMax = 0.8f;

    void PickNewTarget()
    {
        timer = Random.Range(_tMin, _tMax);
        float offset = Random.Range(-_amplitude, _amplitude);
        targetY = Mathf.Clamp(baseY + offset, minY, maxY);
    }
}
