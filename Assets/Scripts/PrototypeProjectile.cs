using UnityEngine;

/// <summary>
/// Temporary player shot. It can later be replaced by a projectile prefab with
/// enemy hit logic and visual effects.
/// </summary>
public sealed class PrototypeProjectile : MonoBehaviour
{
    private static Sprite squareSprite;
    private GameObject owner;

    [SerializeField] private float lifetime = 2.5f;

    public static Sprite GetSquareSprite()
    {
        if (squareSprite == null)
        {
            squareSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            squareSprite.name = "PrototypeWhiteSquare";
        }

        return squareSprite;
    }

    public void SetOwner(GameObject projectileOwner)
    {
        owner = projectileOwner;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner)
        {
            return;
        }

        Destroy(gameObject);
    }
}
