using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PrototypeHealth))]
public sealed class PrototypeEnemyBouncer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float bounceSpeed = 8f;
    [SerializeField] private int startDirection = 1;
    [SerializeField] private LayerMask groundLayer = 1;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private int direction;
    private bool canBounce;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        direction = startDirection < 0 ? -1 : 1;
        canBounce = true;
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(direction * moveSpeed, body.linearVelocity.y);
        if (body.linearVelocity.y > 0.1f)
        {
            canBounce = false;
        }
        else if (body.linearVelocity.y < -0.1f)
        {
            canBounce = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0)
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && canBounce)
            {
                body.linearVelocity = new Vector2(direction * moveSpeed, bounceSpeed);
                canBounce = false;
                return;
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                direction *= -1;
            }
        }
    }
}
