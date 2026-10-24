using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PrototypeHealth))]
public sealed class PrototypeEnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 20f;
    [Min(0.1f)]
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private int startDirection = 1;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private float startX;
    private int direction;

    public int FacingDirection => direction;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startX = transform.position.x;
        direction = startDirection < 0 ? -1 : 1;
    }

    private void FixedUpdate()
    {
        float distanceFromStart = transform.position.x - startX;
        if (Mathf.Abs(distanceFromStart) >= patrolDistance && Mathf.Sign(distanceFromStart) == direction)
        {
            direction *= -1;
        }

        float targetSpeed = direction * moveSpeed;
        float speed = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(speed, body.linearVelocity.y);
        UpdateFacing();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                direction *= -1;
                break;
            }
        }
    }

    private void UpdateFacing()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }
    }
}
