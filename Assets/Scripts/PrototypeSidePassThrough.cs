using System.Collections;
using UnityEngine;

/// <summary>
/// Temporarily ignores a player's collision when entering a platform from its side.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class PrototypeSidePassThrough : MonoBehaviour
{
    [SerializeField] private bool allowSidePassThrough = true;
    [Min(0.05f)]
    [SerializeField] private float ignoreCollisionDuration = 0.25f;

    private BoxCollider2D platformCollider;
    private Coroutine restoreRoutine;

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider2D>();
    }

    public bool TryPassThrough(Collider2D playerCollider)
    {
        if (!allowSidePassThrough || playerCollider == null || platformCollider == null)
        {
            return false;
        }

        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
        }

        restoreRoutine = StartCoroutine(IgnoreCollisionTemporarily(playerCollider));
        return true;
    }

    private IEnumerator IgnoreCollisionTemporarily(Collider2D playerCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(ignoreCollisionDuration);

        if (playerCollider != null && platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }

        restoreRoutine = null;
    }
}
