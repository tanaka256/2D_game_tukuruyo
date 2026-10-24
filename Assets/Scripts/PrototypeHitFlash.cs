using System.Collections;
using UnityEngine;

/// <summary>
/// Briefly flashes a character's sprite when it takes damage.
/// </summary>
public sealed class PrototypeHitFlash : MonoBehaviour
{
    [Min(0.01f)]
    [SerializeField] private float flashDuration = 0.08f;
    [Min(1)]
    [SerializeField] private int flashCount = 2;
    [SerializeField] private Color flashColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Play()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int index = 0; index < flashCount; index++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        flashRoutine = null;
    }
}
