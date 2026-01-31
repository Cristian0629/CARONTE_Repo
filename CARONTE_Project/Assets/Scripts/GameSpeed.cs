using UnityEngine;

public class GameSpeed : MonoBehaviour
{
    public static GameSpeed Instance { get; private set; }

    [Header("Fallback Speed (if no GameSpeed_BG in scene)")]
    [SerializeField] private float speed = 5f;

    // ✅ Mantengo Speed (para HUD)
    public float Speed => CurrentSpeed;

    // ✅ AÑADO CurrentSpeed (para scripts antiguos)
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
        // Si existe tu sistema real de velocidad, lo usamos.
        if (GameSpeed_BG.Instance != null)
            CurrentSpeed = GameSpeed_BG.Instance.CurrentSpeed;
        else
            CurrentSpeed = speed;
    }

    // Por si algún día quieres cambiar speed manualmente (fallback)
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        CurrentSpeed = speed;
    }
}
