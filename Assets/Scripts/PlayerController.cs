using UnityEngine;

// WASD swimming inside the screen, Space dodge (once purchased), knockback + invulnerability on hit.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public Animator animator;
    public SpriteRenderer sprite;

    [Header("Movement")]
    public float acceleration = 25f;
    public float edgeMargin = 0.6f;      // keeps the player this far inside the screen edges

    [Header("Dodge")]
    public float dodgeSpeed = 12f;
    public float dodgeDuration = 0.25f;
    public float dodgeCooldown = 1f;

    [Header("Damage")]
    public float invulnerableTime = 1.5f;
    public float knockbackSpeed = 7f;
    public float stunTime = 0.25f;

    Rigidbody2D rb;
    Camera cam;
    Vector2 input, dodgeDir;
    float dodgeTimer, cooldownTimer, invulnTimer, stunTimer, lastWallSound;
    bool frozen, facingRight = true;

    public bool IsDodging => dodgeTimer > 0f;
    public bool IsInvulnerable => invulnTimer > 0f || dodgeTimer > 0f;
    public bool DodgeReady => cooldownTimer <= 0f;
    public float DodgeCooldownLeft => Mathf.Max(0f, cooldownTimer);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (frozen) { input = Vector2.zero; return; }

        float dt = Time.deltaTime;
        if (dodgeTimer > 0f) dodgeTimer -= dt;
        if (cooldownTimer > 0f) cooldownTimer -= dt;
        if (invulnTimer > 0f) invulnTimer -= dt;
        if (stunTimer > 0f) stunTimer -= dt;

        input = stunTimer > 0f
            ? Vector2.zero
            : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();

        if (input.x > 0.1f) facingRight = true;
        else if (input.x < -0.1f) facingRight = false;

        // Dodge / speed boost
        if (Input.GetKeyDown(KeyCode.Space) && GameManager.Instance.HasDodge
            && cooldownTimer <= 0f && stunTimer <= 0f && dodgeTimer <= 0f)
        {
            dodgeDir = input.sqrMagnitude > 0.01f ? input.normalized : (facingRight ? Vector2.right : Vector2.left);
            dodgeTimer = dodgeDuration;
            cooldownTimer = dodgeCooldown;
            AudioManager.Play(Sfx.Dodge);
        }

        if (sprite)
        {
            sprite.flipX = !facingRight;   // assumes the sprite faces right by default
            float a = 1f;
            if (dodgeTimer > 0f) a = 0.6f;
            else if (invulnTimer > 0f && Mathf.FloorToInt(Time.time * 15f) % 2 == 0) a = 0.35f;
            var c = sprite.color; c.a = a; sprite.color = c;
        }

        if (animator) animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }

    void FixedUpdate()
    {
        if (frozen) { rb.linearVelocity = Vector2.zero; return; }

        var gm = GameManager.Instance;
        Vector2 vel = rb.linearVelocity;  

        if (dodgeTimer > 0f) vel = dodgeDir * dodgeSpeed;
        else if (stunTimer > 0f) vel = Vector2.MoveTowards(vel, Vector2.zero, 10f * Time.fixedDeltaTime);
        else vel = Vector2.MoveTowards(vel, input * gm.SwimSpeed, acceleration * Time.fixedDeltaTime);

        // Keep the player inside the screen; the top is the water line
        Vector2 pos = rb.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;
        float minX = c.x - halfW + edgeMargin;
        float maxX = c.x + halfW - edgeMargin;
        float minY = c.y - halfH + edgeMargin;
        float maxY = gm.surfaceY - 0.3f;
        bool bumped = false;

        if (pos.x < minX) { pos.x = minX; if (vel.x < 0f) { bumped |= vel.x < -2.5f; vel.x = 0f; } }
        if (pos.x > maxX) { pos.x = maxX; if (vel.x > 0f) { bumped |= vel.x > 2.5f; vel.x = 0f; } }
        if (pos.y < minY) { pos.y = minY; if (vel.y < 0f) { bumped |= vel.y < -2.5f; vel.y = 0f; } }
        if (pos.y > maxY) { pos.y = maxY; if (vel.y > 0f) vel.y = 0f; }

        if (pos != rb.position) rb.position = pos;
        rb.linearVelocity = vel;

        if (bumped && Time.time - lastWallSound > 0.3f)
        {
            lastWallSound = Time.time;
            AudioManager.Play(Sfx.HitWall);
        }
    }

    public void Hurt(Vector2 sourcePos)
    {
        invulnTimer = invulnerableTime;
        stunTimer = stunTime;
        dodgeTimer = 0f;
        Vector2 dir = ((Vector2)transform.position - sourcePos).normalized;
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.down;
        rb.linearVelocity = dir * knockbackSpeed;
    }

    public void Respawn(Vector2 pos)
    {
        transform.position = pos;
        rb.position = pos;
        rb.linearVelocity = Vector2.zero;
        dodgeTimer = cooldownTimer = invulnTimer = stunTimer = 0f;
        facingRight = true;
        frozen = false;
        SetVisible(true);
    }

    public void SetFrozen(bool value)
    {
        frozen = value;
        if (value) rb.linearVelocity = Vector2.zero;
    }

    public void SetVisible(bool visible)
    {
        if (sprite) sprite.enabled = visible;
    }
}
