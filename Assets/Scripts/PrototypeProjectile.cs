using UnityEngine;

/// <summary>
/// Configurable projectile that damages opposing factions and then disappears.
/// </summary>
public sealed class PrototypeProjectile : MonoBehaviour
{
    private static Sprite squareSprite;
    private GameObject owner;

    [SerializeField] private float lifetime = 2.5f;
    [Min(1)]
    [SerializeField] private int damage = 1;
    [SerializeField] private PrototypeFaction faction = PrototypeFaction.Player;

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

    public void SetDamage(int value)
    {
        damage = Mathf.Max(1, value);
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

        PrototypeHealth target = other.GetComponentInParent<PrototypeHealth>();
        if (target != null && target.Faction != faction)
        {
            GameObject source = owner != null ? owner : gameObject;
            PrototypeFrontDefense defense = target.GetComponent<PrototypeFrontDefense>();
            if (defense != null && defense.BlocksAttackFrom(source))
            {
                target.GetComponent<PrototypeAudioEvents>()?.PlayGuardBlock();
            }
            else
            {
                target.TakeDamage(damage, source);
            }
        }

        Destroy(gameObject);
    }
}
