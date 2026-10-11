using UnityEngine;

/// <summary>
/// A platform that can be passed through from below but supports characters from above.
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public sealed class PrototypeOneWayPlatform : MonoBehaviour
{
    private void Reset()
    {
        Configure();
    }

    private void Awake()
    {
        Configure();
    }

    private void Configure()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.usedByEffector = true;

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.surfaceArc = 180f;
    }
}
