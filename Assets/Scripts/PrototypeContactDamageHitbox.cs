using UnityEngine;

/// <summary>
/// Trigger child used by PrototypeContactDamage.
/// </summary>
public sealed class PrototypeContactDamageHitbox : MonoBehaviour
{
    private PrototypeContactDamage owner;

    public void SetOwner(PrototypeContactDamage damageOwner)
    {
        owner = damageOwner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        owner?.TryDamage(other);
    }
}
