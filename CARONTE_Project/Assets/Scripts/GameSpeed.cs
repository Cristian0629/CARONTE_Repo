using UnityEngine;

public class GameSpeed : MonoBehaviour
{
    public static GameSpeed Instance { get; private set; }

    [Header("Fallback Speed (if no GameSpeed_BG in scene)")]
    [SerializeField] private float speed = 5f;

    
    public float Speed => CurrentSpeed;

    
    public float CurrentSpeed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        
        if (GameSpeed_BG.Instance != null)
            CurrentSpeed = GameSpeed_BG.Instance.CurrentSpeed;
        else
            CurrentSpeed = speed;
    }

    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        CurrentSpeed = speed;
    }
}
