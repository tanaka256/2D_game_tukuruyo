using UnityEngine;

/// <summary>
/// Shared projectile spawning for enemy shooting behaviours.
/// </summary>
public sealed class PrototypeEnemyProjectileShooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 8f;
    [Min(1)]
    [SerializeField] private int projectileDamage = 1;

    public float ProjectileSpeed => projectileSpeed;
    public int ProjectileDamage => projectileDamage;

    public bool FireAt(Vector2 targetPosition)
    {
        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;
        return FireAt(targetPosition, spawnPosition);
    }

    /// <summary>
    /// Fires from the shooter's center toward the target. Useful for turrets
    /// that can aim in every direction instead of using a fixed muzzle point.
    /// </summary>
    public bool FireAtFromCenter(Vector2 targetPosition, float muzzleDistance)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        float safeMuzzleDistance = Mathf.Min(
            Mathf.Max(0f, muzzleDistance),
            direction.magnitude * 0.5f);
        Vector3 spawnPosition = transform.position
            + (Vector3)(direction.normalized * safeMuzzleDistance);
        return FireAt(targetPosition, spawnPosition);
    }

    private bool FireAt(Vector2 targetPosition, Vector3 spawnPosition)
    {
        if (projectilePrefab == null)
        {
            return false;
        }

        Vector2 direction = targetPosition - (Vector2)spawnPosition;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D projectileBody = projectileObject.GetComponent<Rigidbody2D>();
        if (projectileBody == null)
        {
            projectileBody = projectileObject.AddComponent<Rigidbody2D>();
        }

        projectileBody.gravityScale = 0f;
        projectileBody.bodyType = RigidbodyType2D.Dynamic;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        projectileBody.linearVelocity = direction.normalized * projectileSpeed;

        PrototypeProjectile projectile = projectileObject.GetComponent<PrototypeProjectile>();
        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<PrototypeProjectile>();
        }

        projectile.SetOwner(gameObject);
        projectile.SetDamage(projectileDamage);
        GetComponent<PrototypeAudioEvents>()?.PlayEnemyShot();
        return true;
    }
}
