using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWaveRide : MonoBehaviour
{
    // ✅ NUEVO: singleton simple para que las monedas puedan encontrar al player
    public static PlayerWaveRide Instance { get; private set; }

    [Header("References")]
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundCheckRadius = 0.18f;

    [Header("Magnet Target")]
    public Transform MagnetPoint;

    [Header("Upward feel")]
    public float liftAcceleration = 125f;
    public float maxUpSpeed = 13f;

    [Header("Fall feel")]
    public float maxDownSpeed = 4.8f;
    public float gravityScale = 1.5f;

    [Header("Takeoff from ground (small delay then strong)")]
    public float takeoffRampTime = 0.12f;
    public float takeoffStartMultiplier = 0.25f;

    [Header("Re-press in air (fast response)")]
    public float reEngageLiftMultiplier = 1.8f;
    public float reEngageDuration = 0.18f;

    [Header("Smooth fall cancel (no brusco)")]
    public float fallCancelTo = -0.6f;
    public float fallCancelRate = 120f;
    public float fallCancelTime = 0.20f;

    [Header("Re-engage kick (makes it rise sooner, still smooth)")]
    public float reEngageKickUpSpeed = 1.8f;
    public float reEngageKickRate = 55f;
    public float reEngageKickTime = 0.14f;

    // ✅ NUEVO: Magnet config
    [Header("Magnet PowerUp")]
    [SerializeField] private float magnetRadius = 3.5f;
    [SerializeField] private float magnetPullSpeed = 18f;

    public bool MagnetActive { get; private set; }
    public float MagnetRadius => magnetRadius;
    public float MagnetPullSpeed => magnetPullSpeed;

    private Coroutine magnetRoutine;

    // ✅ NUEVO: Feedback al coger monedas (flash)
    [Header("Coin Pickup Feedback (Flash)")]
    [Tooltip("Si lo dejas vacío, se auto-detectan SpriteRenderers en el player y sus hijos.")]
    [SerializeField] private SpriteRenderer[] flashRenderers;

    [Tooltip("Color del flash (blanco = ilumina).")]
    [SerializeField] private Color flashColor = Color.white;

    [Tooltip("Intensidad del flash (0 = nada, 1 = fuerte).")]
    [Range(0f, 1f)]
    [SerializeField] private float flashStrength = 0.35f;

    [Tooltip("Duración total del flash (segundos).")]
    [SerializeField] private float flashDuration = 0.10f;

    private Color[] baseColors;
    private Coroutine flashRoutine;
    private int flashToken;

    private Rigidbody2D rb;
    private bool wasHolding;
    private float takeoffTimer;
    private float reEngageTimer;
    private float fallCancelTimer;
    private float kickTimer;

    void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = gravityScale;
        rb.linearDamping = 0f;

        // ✅ NUEVO: preparar renderers del flash
        if (flashRenderers == null || flashRenderers.Length == 0)
            flashRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (flashRenderers != null && flashRenderers.Length > 0)
        {
            baseColors = new Color[flashRenderers.Length];
            for (int i = 0; i < flashRenderers.Length; i++)
                baseColors[i] = (flashRenderers[i] != null) ? flashRenderers[i].color : Color.white;
        }
    }

    // ✅ NUEVO: LLAMAR desde las monedas cuando se recogen
    public void OnCoinCollected()
    {
        TriggerFlash();
    }

    private void TriggerFlash()
    {
        if (flashRenderers == null || flashRenderers.Length == 0) return;
        if (baseColors == null || baseColors.Length != flashRenderers.Length) return;
        if (flashDuration <= 0f || flashStrength <= 0f) return;

        flashToken++;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(flashToken));
    }

    private IEnumerator FlashRoutine(int token)
    {
        float half = flashDuration * 0.5f;
        if (half <= 0f) yield break;

        // subir (base -> flash)
        float t = 0f;
        while (t < half)
        {
            if (token != flashToken) yield break;

            t += Time.unscaledDeltaTime; // consistente aunque haya slowmo
            float p = Mathf.Clamp01(t / half);
            ApplyFlash(p);
            yield return null;
        }

        // bajar (flash -> base)
        t = 0f;
        while (t < half)
        {
            if (token != flashToken) yield break;

            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / half);
            ApplyFlash(1f - p);
            yield return null;
        }

        RestoreBaseColors();
        flashRoutine = null;
    }

    private void ApplyFlash(float amount01)
    {
        float a = Mathf.Clamp01(amount01) * flashStrength;

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var sr = flashRenderers[i];
            if (sr == null) continue;

            Color baseC = baseColors[i];
            // Mezcla hacia blanco (o el color que elijas) para “iluminar”
            sr.color = Color.Lerp(baseC, flashColor, a);
        }
    }

    private void RestoreBaseColors()
    {
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var sr = flashRenderers[i];
            if (sr == null) continue;
            sr.color = baseColors[i];
        }
    }

    // ✅ NUEVO: activar imán X segundos (reinicia si lo pillas otra vez)
    public void ActivateMagnet(float durationSeconds)
    {
        if (magnetRoutine != null) StopCoroutine(magnetRoutine);
        magnetRoutine = StartCoroutine(MagnetRoutine(durationSeconds));
    }

    private IEnumerator MagnetRoutine(float duration)
    {
        MagnetActive = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        MagnetActive = false;
        magnetRoutine = null;
    }

    void FixedUpdate()
    {
        bool holding = Input.GetKey(KeyCode.Space);
        bool grounded = IsGrounded();

        if (holding && !wasHolding)
        {
            if (!grounded)
            {
                fallCancelTimer = fallCancelTime;
                reEngageTimer = reEngageDuration;
                kickTimer = reEngageKickTime;
            }
            else
            {
                takeoffTimer = 0f;
            }
        }

        if (fallCancelTimer > 0f)
        {
            if (rb.linearVelocity.y < fallCancelTo)
            {
                float newY = Mathf.MoveTowards(rb.linearVelocity.y, fallCancelTo, fallCancelRate * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
            }
            fallCancelTimer -= Time.fixedDeltaTime;
        }

        if (kickTimer > 0f)
        {
            float newY = Mathf.MoveTowards(rb.linearVelocity.y, reEngageKickUpSpeed, reEngageKickRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
            kickTimer -= Time.fixedDeltaTime;
        }

        if (holding)
        {
            float accel = liftAcceleration;

            if (grounded)
            {
                takeoffTimer += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(takeoffTimer / takeoffRampTime);
                float ramp = Mathf.Lerp(takeoffStartMultiplier, 1f, t);
                accel *= ramp;
            }
            else if (reEngageTimer > 0f)
            {
                accel *= reEngageLiftMultiplier;
                reEngageTimer -= Time.fixedDeltaTime;
            }

            rb.AddForce(Vector2.up * accel, ForceMode2D.Force);

            if (rb.linearVelocity.y > maxUpSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxUpSpeed);
        }
        else
        {
            takeoffTimer = 0f;
            reEngageTimer = 0f;
            fallCancelTimer = 0f;
            kickTimer = 0f;
        }

        if (rb.linearVelocity.y < -maxDownSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxDownSpeed);

        wasHolding = holding;
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // ✅ NUEVO: gizmo del imán (solo para ver el radio)
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
