using System.Collections;
using UnityEngine;

/// <summary>
/// Breaks after a character lands on it, then optionally respawns.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class PrototypeBreakablePlatform : MonoBehaviour
{
    [Min(0f)]
    [SerializeField] private float breakDelay = 0.75f;
    [SerializeField] private bool respawn = true;
    [Min(0.1f)]
    [SerializeField] private float respawnDelay = 3f;

    private BoxCollider2D platformCollider;
    private SpriteRenderer spriteRenderer;
    private bool countdownStarted;

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
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
