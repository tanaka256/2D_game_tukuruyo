using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PrototypeHealth))]
public sealed class PrototypeEnemyChaser : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float stopDistance = 0.1f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Transform target;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (target == null)
        {
            PrototypePlayerController player = FindFirstObjectByType<PrototypePlayerController>();
            target = player != null ? player.transform : null;
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        float delta = target.position.x - transform.position.x;
        float direction = Mathf.Abs(delta) <= stopDistance ? 0f : Mathf.Sign(delta);
        float targetSpeed = direction * moveSpeed;
        float speed = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(speed, body.linearVelocity.y);

        if (direction != 0f && spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }
}
