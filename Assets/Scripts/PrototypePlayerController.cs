using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Minimal 2D platformer controller for the first playable prototype.
/// Replace this component when the player's movement and action design is ready.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float jumpSpeed = 11f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer = 1;

    [Header("Jump Upgrades")]
    [Min(1)]
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private bool unlimitedJumpUnlocked;

    [Header("Shooting")]
    [SerializeField] private Vector2 projectileSpawnOffset = new(0.58f, 0.08f);
    [SerializeField] private float projectileSpeed = 13f;
    [SerializeField] private float shotCooldown = 0.2f;

    private Rigidbody2D body;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform projectileSpawnPoint;
    private readonly HashSet<Collider2D> groundContacts = new();
    private int jumpsRemaining;
    private bool grounded;
    private bool hasJumpedSinceLanding;
    private bool jumpInProgress;
    private int facingDirection = 1;
    private float nextShotTime;

    public int MaxJumps => maxJumps;
    public int JumpsRemaining => unlimitedJumpUnlocked ? -1 : jumpsRemaining;
    public bool UnlimitedJumpUnlocked => unlimitedJumpUnlocked;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxJumps = Mathf.Max(1, maxJumps);
        jumpsRemaining = maxJumps;

        if (projectileSpawnPoint == null)
        {
            GameObject spawnPointObject = new("ProjectileSpawnPoint");
            spawnPointObject.transform.SetParent(transform);
            spawnPointObject.transform.localPosition = projectileSpawnOffset;
            projectileSpawnPoint = spawnPointObject.transform;
        }
    }

    private void Update()
    {
        if (WasJumpPressed() && CanJump())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
            if (!unlimitedJumpUnlocked)
            {
                jumpsRemaining--;
            }

            hasJumpedSinceLanding = true;
            jumpInProgress = true;
        }

        if (WasShootPressed() && Time.time >= nextShotTime)
        {
            FireProjectile();
            nextShotTime = Time.time + shotCooldown;
        }
    }

    private void FixedUpdate()
    {
        float horizontalInput = ReadHorizontalInput();
        float targetSpeed = horizontalInput * moveSpeed;
        float newSpeed = Mathf.MoveTowards(
            body.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime);

        body.linearVelocity = new Vector2(newSpeed, body.linearVelocity.y);

        if (horizontalInput != 0f)
        {
            facingDirection = horizontalInput > 0f ? 1 : -1;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = facingDirection < 0;
            }
        }

        if (transform.position.y < -10f)
        {
            transform.position = new Vector3(0f, 2f, 0f);
            body.linearVelocity = Vector2.zero;
            jumpsRemaining = maxJumps;
            groundContacts.Clear();
            grounded = false;
            hasJumpedSinceLanding = false;
            jumpInProgress = false;
        }

        UpdateLandingState();
    }

    private void UpdateLandingState()
    {
        grounded = groundContacts.Count > 0;

        if (grounded && jumpInProgress && body.linearVelocity.y <= 0.1f)
        {
            jumpsRemaining = maxJumps;
            hasJumpedSinceLanding = false;
            jumpInProgress = false;
        }
    }

    private bool CanJump()
    {
        if (unlimitedJumpUnlocked)
        {
            return true;
        }

        if (grounded)
        {
            return !hasJumpedSinceLanding;
        }

        return hasJumpedSinceLanding && jumpsRemaining > 0;
    }

    /// <summary>
    /// Sets the total number of jumps available between landings.
    /// 1 = normal jump, 2 = double jump, 3 = triple jump, and so on.
    /// </summary>
    public void SetMaxJumps(int jumpCount)
    {
        maxJumps = Mathf.Max(1, jumpCount);
        int jumpsAlreadyUsed = hasJumpedSinceLanding ? 1 : 0;
        jumpsRemaining = Mathf.Max(jumpsRemaining, maxJumps - jumpsAlreadyUsed);
        if (grounded)
        {
            jumpsRemaining = maxJumps;
            hasJumpedSinceLanding = false;
            jumpInProgress = false;
        }
    }

    public void UnlockDoubleJump()
    {
        SetMaxJumps(2);
    }

    public void AddExtraJump()
    {
        SetMaxJumps(maxJumps + 1);
    }

    public void SetUnlimitedJump(bool unlocked)
    {
        unlimitedJumpUnlocked = unlocked;
        if (!unlocked && grounded)
        {
            jumpsRemaining = maxJumps;
            hasJumpedSinceLanding = false;
            jumpInProgress = false;
        }
    }

    public void ResetJumpUpgrades()
    {
        maxJumps = 1;
        unlimitedJumpUnlocked = false;
        jumpsRemaining = maxJumps;
        hasJumpedSinceLanding = false;
        jumpInProgress = false;
    }

    private float ReadHorizontalInput()
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        float input = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            input -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            input += 1f;
        }

        return input;
    }

    private bool WasJumpPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.spaceKey.wasPressedThisFrame
            || Keyboard.current.wKey.wasPressedThisFrame
            || Keyboard.current.upArrowKey.wasPressedThisFrame;
    }

    private bool WasShootPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.jKey.wasPressedThisFrame
            || Keyboard.current.xKey.wasPressedThisFrame
            || Keyboard.current.rightCtrlKey.wasPressedThisFrame;
    }

    private void FireProjectile()
    {
        Vector3 localOffset = projectileSpawnPoint.localPosition;
        localOffset.x = Mathf.Abs(localOffset.x) * facingDirection;
        Vector3 spawnPosition = transform.TransformPoint(localOffset);
        GameObject projectileObject = new("PlayerProjectile");
        projectileObject.transform.position = spawnPosition;

        SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
        renderer.sprite = PrototypeProjectile.GetSquareSprite();
        renderer.color = new Color(1f, 0.85f, 0.25f);
        projectileObject.transform.localScale = new Vector3(0.28f, 0.12f, 1f);

        BoxCollider2D collider = projectileObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D projectileBody = projectileObject.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        projectileBody.bodyType = RigidbodyType2D.Dynamic;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        projectileBody.linearVelocity = new Vector2(projectileSpeed * facingDirection, 0f);

        PrototypeProjectile projectile = projectileObject.AddComponent<PrototypeProjectile>();
        projectile.SetOwner(gameObject);
    }

    private bool IsGroundContact(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0)
        {
            return false;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundContact(collision))
        {
            groundContacts.Add(collision.collider);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        groundContacts.Remove(collision.collider);
        if (IsGroundContact(collision))
        {
            groundContacts.Add(collision.collider);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundContacts.Remove(collision.collider);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            return;
        }

        Bounds bounds = collider.bounds;
        Vector2 center = new(bounds.center.x, bounds.min.y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector2(bounds.size.x * 0.9f, 0.08f));
    }
}
