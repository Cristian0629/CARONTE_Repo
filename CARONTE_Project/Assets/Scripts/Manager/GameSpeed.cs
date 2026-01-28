using UnityEngine;

public class GameSpeed : MonoBehaviour
{
    public static GameSpeed Instance;

    [Header("Speed Settings")]
    public float startSpeed = 4f;
    public float maxSpeed = 10f;
    public float acceleration = 0.25f; 

    public float CurrentSpeed { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        CurrentSpeed = startSpeed;
    }

    void Update()
    {
        
        CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, maxSpeed, acceleration * Time.deltaTime);
    }

    public void ResetSpeed()
    {
        CurrentSpeed = startSpeed;
    }
}
