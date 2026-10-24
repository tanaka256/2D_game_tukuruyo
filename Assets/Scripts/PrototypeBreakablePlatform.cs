using System.Collections;
using UnityEngine;

/// <summary>
/// Breaks after a character lands on it, then optionally respawns.
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public sealed class PrototypeBreakablePlatform : MonoBehaviour
{
    [Min(0f)]
    [SerializeField] private float breakDelay = 0.75f;
    [SerializeField] private bool respawn = true;
    [Min(0.1f)]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private bool passThroughFromBelow;
    [Range(0f, 360f)]
    [SerializeField] private float surfaceArc = 180f;

    private BoxCollider2D platformCollider;
    private SpriteRenderer spriteRenderer;
    private bool countdownStarted;

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider.usedByEffector = true;
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleLanding(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleLanding(collision);
    }

    private void HandleLanding(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<PrototypePlayerController>() == null)
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            bool contactIsOnTop = contact.point.y >= platformCollider.bounds.max.y - 0.08f;
            if (contactIsOnTop)
            {
                StartBreakCountdown();
                return;
            }
        }
    }

    private void StartBreakCountdown()
    {
        if (!countdownStarted)
        {
            StartCoroutine(BreakRoutine());
        }
    }

    private IEnumerator BreakRoutine()
    {
        countdownStarted = true;
        yield return new WaitForSeconds(breakDelay);

        if (!respawn)
        {
            Destroy(gameObject);
            yield break;
        }

        platformCollider.enabled = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        platformCollider.enabled = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        countdownStarted = false;
    }
}
