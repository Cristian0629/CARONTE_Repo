using UnityEngine;

public class GameSpeed_BG : MonoBehaviour
{
    public static GameSpeed_BG Instance;

    

    [Header("Speed Settings")]
    public float startSpeed = 5.2f;
    public float maxSpeed = 9.5f;

    [Tooltip("Tiempo (en segundos) para llegar cerca del maximo")]
    public float timeToMaxSeconds = 270f; // 4.5 min

    [Header("How fast difficulty grows (more noticeable early)")]
    public AnimationCurve difficultyCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.25f, 0.55f),
        new Keyframe(1f, 1f)
    );

    public float CurrentSpeed { get; private set; }
    [SerializeField] private float debugCurrentSpeed;

    public float Difficulty01 { get; private set; }

    private float t;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        ResetSpeed();
    }

    void Update()
    {

        t += Time.deltaTime / Mathf.Max(0.01f, timeToMaxSeconds);
        Difficulty01 = Mathf.Clamp01(t);

        float curveT = difficultyCurve.Evaluate(Difficulty01);
        CurrentSpeed = Mathf.Lerp(startSpeed, maxSpeed, curveT);

        debugCurrentSpeed = CurrentSpeed;
    }

    public void ResetSpeed()
    {
        t = 0f;
        CurrentSpeed = startSpeed;
        Difficulty01 = 0f;
        debugCurrentSpeed = CurrentSpeed;
    }

    public void ReduceSpeed(float amount)
    {
        CurrentSpeed = Mathf.Max(startSpeed, CurrentSpeed - Mathf.Abs(amount));
        debugCurrentSpeed = CurrentSpeed;
    }

    public void RewindDifficultySeconds(float seconds)
    {
        float delta = Mathf.Abs(seconds) / Mathf.Max(0.01f, timeToMaxSeconds);
        t = Mathf.Clamp01(t - delta);
        Difficulty01 = Mathf.Clamp01(t);

        float curveT = difficultyCurve.Evaluate(Difficulty01);
        CurrentSpeed = Mathf.Lerp(startSpeed, maxSpeed, curveT);
        debugCurrentSpeed = CurrentSpeed;
    }
}
