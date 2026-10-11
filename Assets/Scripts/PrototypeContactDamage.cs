using UnityEngine;

/// <summary>
/// Separates enemy/player physical collision from the damage hitbox.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class PrototypeContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [Min(1)]
    [SerializeField] private int damage = 1;
    [Min(0.05f)]
    [SerializeField] private float damageCooldown = 0.75f;
    [SerializeField] private bool contactDamageEnabled = true;

    [Header("Physical Collision")]
    [SerializeField] private bool physicalCollisionEnabled = true;

    [Header("Damage Hitbox")]
    [SerializeField] private Vector2 hitboxSize = new(1f, 1f);
    [SerializeField] private Vector2 hitboxOffset;
    [SerializeField] private bool showHitboxGizmo = true;
    [SerializeField] private Color hitboxGizmoColor = new(1f, 0.1f, 0.1f, 0.8f);

    private float nextDamageTime;
    private PrototypeHealth ownerHealth;
    private Collider2D physicalCollider;
    private BoxCollider2D hitboxCollider;

    public bool ContactDamageEnabled => contactDamageEnabled;
    public bool PhysicalCollisionEnabled => physicalCollisionEnabled;
    public Vector2 HitboxSize => hitboxSize;
    public Vector2 HitboxOffset => hitboxOffset;

    private void Awake()
    {
        ownerHealth = GetComponent<PrototypeHealth>();
        physicalCollider = GetComponent<Collider2D>();
        EnsureHitbox();
    }

    private void Start()
    {
        ApplyPlayerCollisionSetting();
    }

    private void FixedUpdate()
    {
        ApplyPlayerCollisionSetting();
    }

    public void EnsureHitbox()
    {
        if (hitboxCollider == null)
        {
            Transform hitboxTransform = transform.Find("ContactDamageHitbox");
            if (hitboxTransform == null)
            {
                GameObject hitboxObject = new("ContactDamageHitbox");
                hitboxTransform = hitboxObject.transform;
                hitboxTransform.SetParent(transform, false);
            }

            hitboxTransform.localPosition = hitboxOffset;
            hitboxCollider = hitboxTransform.GetComponent<BoxCollider2D>();
            if (hitboxCollider == null)
            {
                hitboxCollider = hitboxTransform.gameObject.AddComponent<BoxCollider2D>();
            }

            hitboxCollider.isTrigger = true;
            PrototypeContactDamageHitbox hitbox = hitboxTransform.GetComponent<PrototypeContactDamageHitbox>();
            if (hitbox == null)
            {
                hitbox = hitboxTransform.gameObject.AddComponent<PrototypeContactDamageHitbox>();
            }

            hitbox.SetOwner(this);
        }

        hitboxCollider.size = hitboxSize;
        hitboxCollider.offset = Vector2.zero;
        hitboxCollider.enabled = contactDamageEnabled;
        hitboxCollider.transform.localPosition = hitboxOffset;
    }

    public void TryDamage(Collider2D other)
    {
        if (!contactDamageEnabled || Time.time < nextDamageTime || ownerHealth == null)
        {
            return;
        }

        PrototypeHealth target = other.GetComponentInParent<PrototypeHealth>();
        if (target == null || target == ownerHealth || target.Faction == ownerHealth.Faction)
        {
            return;
        }

        target.TakeDamage(damage, gameObject);
        nextDamageTime = Time.time + damageCooldown;
    }

    private void ApplyPlayerCollisionSetting()
    {
        if (physicalCollider == null)
        {
            return;
        }

        PrototypePlayerController[] players = FindObjectsByType<PrototypePlayerController>(FindObjectsSortMode.None);
        foreach (PrototypePlayerController player in players)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(physicalCollider, playerCollider, !physicalCollisionEnabled);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showHitboxGizmo)
        {
            return;
        }

        Gizmos.color = hitboxGizmoColor;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(hitboxOffset, hitboxSize);
        Gizmos.matrix = previousMatrix;
    }
}
