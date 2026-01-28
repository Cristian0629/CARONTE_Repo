using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Movement")]
    public float forwardSpeedOffset = 0f;     
    public float maxVerticalSpeed = 6f;       
    public float turnRate = 8f;              

    [Header("Behavior")]
    public float homingDelay = 0.5f;        
    public float deadZone = 0.25f;         
    public float wobble = 0.15f;             

    private float delayTimer;


    void Update()
    {
     
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

       
        float baseSpeed = (GameSpeed.Instance != null) ? GameSpeed.Instance.CurrentSpeed : 4f;
        float forwardSpeed = baseSpeed + forwardSpeedOffset;

        Vector3 pos = transform.position;
        pos.x -= forwardSpeed * Time.deltaTime;

  
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            transform.position = pos;
            return;
        }

        if (target != null)
        {
            float dy = target.position.y - pos.y;

            if (Mathf.Abs(dy) < deadZone) dy = 0f;

            dy += Mathf.Sin(Time.time * 6f) * wobble;

            float desiredVy = Mathf.Clamp(dy * 2.0f, -maxVerticalSpeed, maxVerticalSpeed);

            float currentVy = 0f;
            currentVy = Mathf.MoveTowards(currentVy, desiredVy, turnRate * Time.deltaTime);

            pos.y += currentVy * Time.deltaTime;
        }

        transform.position = pos;
    }
}