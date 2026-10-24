using System.Collections;
using UnityEngine;

/// <summary>
/// One-way platform that also allows the player to drop through while crouching.
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public sealed class PrototypeDropThroughPlatform : MonoBehaviour
{
    [Min(0.1f)]
    [SerializeField] private float ignoreCollisionDuration = 0.35f;
    [SerializeField] private bool passThroughFromBelow = true;
    [SerializeField] private bool allowPlayerDropThrough = true;
    [Range(0f, 360f)]
    [SerializeField] private float surfaceArc = 180f;

    private BoxCollider2D platformCollider;

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider2D>();
        platformCollider.usedByEffector = true;

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        effector.useOneWay = passThroughFromBelow;
        effector.surfaceArc = surfaceArc;
        if (GetComponent<PrototypeSidePassThrough>() == null)
        {
            gameObject.AddComponent<PrototypeSidePassThrough>();
        }
    }

    public bool TryDropThrough(Collider2D playerCollider)
    {
        if (!allowPlayerDropThrough || playerCollider == null || !platformCollider.enabled)
        {
            return false;
        }

        StartCoroutine(TemporarilyIgnoreCollision(playerCollider));
        return true;
    }

    private IEnumerator TemporarilyIgnoreCollision(Collider2D playerCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(ignoreCollisionDuration);

        if (playerCollider != null && platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }
}
