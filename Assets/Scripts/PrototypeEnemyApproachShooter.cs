using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PrototypeEnemyProjectileShooter))]
public sealed class PrototypeEnemyApproachShooter : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float approachSpeed = 2.5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float shootingDistance = 5f;
    [SerializeField] private float retreatDistance = 3.5f;

    [Header("Shooting")]
    [SerializeField] private float shotInterval = 1.1f;
    [Min(0f)]
    [SerializeField] private float muzzleDistance = 0.8f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private PrototypeEnemyProjectileShooter shooter;
    private PrototypeEnemyActivation activation;
    private Transform target;
    private float nextShotTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        shooter = GetComponent<PrototypeEnemyProjectileShooter>();
        activation = GetComponent<PrototypeEnemyActivation>();
    }

    private void Update()
    {
        if (target == null)
        {
            PrototypePlayerController player = FindFirstObjectByType<PrototypePlayerController>();
            target = player != null ? player.transform : null;
        }

        if (target == null || (activation != null && !activation.IsActive))
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= shootingDistance && Time.time >= nextShotTime)
        {
            if (shooter.FireAtFromCenter(target.position, muzzleDistance))
            {
                nextShotTime = Time.time + Mathf.Max(0.05f, shotInterval);
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null || (activation != null && !activation.IsActive))
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        float horizontalDelta = target.position.x - transform.position.x;
        float absoluteDistance = Mathf.Abs(horizontalDelta);
        float desiredDistance = absoluteDistance < shootingDistance ? retreatDistance : shootingDistance;
        float direction = Mathf.Abs(horizontalDelta) <= desiredDistance
            ? 0f
            : Mathf.Sign(horizontalDelta);
        float speed = Mathf.MoveTowards(
            body.linearVelocity.x,
            direction * approachSpeed,
            acceleration * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(speed, body.linearVelocity.y);

        if (direction != 0f && spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }
}
