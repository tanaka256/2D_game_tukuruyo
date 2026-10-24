using UnityEngine;

/// <summary>
/// A platform that can be passed through from below but supports characters from above.
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public sealed class PrototypeOneWayPlatform : MonoBehaviour
{
    [SerializeField] private bool passThroughFromBelow = true;
    [Range(0f, 360f)]
    [SerializeField] private float surfaceArc = 180f;

    private void Reset()
    {
        Configure();
    }

    private void Awake()
    {
        Configure();
        EnsureSidePassThrough();
    }

    private void Configure()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.usedByEffector = true;

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        effector.useOneWay = passThroughFromBelow;
        effector.surfaceArc = surfaceArc;
    }

    private void EnsureSidePassThrough()
    {
        if (GetComponent<PrototypeSidePassThrough>() == null)
        {
            gameObject.AddComponent<PrototypeSidePassThrough>();
        }
    }
}
