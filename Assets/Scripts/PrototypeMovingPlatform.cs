using UnityEngine;

public enum PrototypeMovingPlatformAxis
{
    Horizontal,
    Vertical
}

/// <summary>
/// Moves a kinematic platform between its start position and a configurable endpoint.
/// Rigidbody2D movement carries standing characters with the platform.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public sealed class PrototypeMovingPlatform : MonoBehaviour
{
    [SerializeField] private PrototypeMovingPlatformAxis axis = PrototypeMovingPlatformAxis.Horizontal;
    [Min(0.1f)]
    [SerializeField] private float speed = 2f;
    [Min(0.1f)]
    [SerializeField] private float travelDistance = 3f;
    [Min(0f)]
    [SerializeField] private float waitAtEnds;
    [SerializeField] private bool passThroughFromBelow;
    [Range(0f, 360f)]
    [SerializeField] private float surfaceArc = 180f;

    private Rigidbody2D body;
    private Vector2 startPosition;
    private Vector2 previousPosition;
    private int direction = 1;
    private float waitTimer;

    public Vector2 CurrentVelocity { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.useFullKinematicContacts = true;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.usedByEffector = true;
        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
        {
            effector = gameObject.AddComponent<PlatformEffector2D>();
        }
        effector.useOneWay = passThroughFromBelow;
        effector.surfaceArc = surfaceArc;
        if (GetComponent<PrototypeSidePassThrough>() == null)
        {
            gameObject.AddComponent<PrototypeSidePassThrough>();
        }
        startPosition = body.position;
        previousPosition = startPosition;
    }

    private void FixedUpdate()
    {
        Vector2 endPosition = startPosition + (axis == PrototypeMovingPlatformAxis.Horizontal
            ? Vector2.right
            : Vector2.up) * travelDistance;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            CurrentVelocity = Vector2.zero;
            return;
        }

        Vector2 target = direction > 0 ? endPosition : startPosition;
        Vector2 nextPosition = Vector2.MoveTowards(body.position, target, speed * Time.fixedDeltaTime);
        body.MovePosition(nextPosition);
        CurrentVelocity = (nextPosition - previousPosition) / Time.fixedDeltaTime;
        previousPosition = nextPosition;

        if (Vector2.Distance(nextPosition, target) < 0.001f)
        {
            direction *= -1;
            waitTimer = waitAtEnds;
        }
    }
}
