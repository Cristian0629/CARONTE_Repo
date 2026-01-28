using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWaveRide : MonoBehaviour
{
    [Header("References")]
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundCheckRadius = 0.18f;

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

    private Rigidbody2D rb;
    private bool wasHolding;
    private float takeoffTimer;
    private float reEngageTimer;
    private float fallCancelTimer;
    private float kickTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = gravityScale;
        rb.linearDamping = 0f; 
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
    }
}
