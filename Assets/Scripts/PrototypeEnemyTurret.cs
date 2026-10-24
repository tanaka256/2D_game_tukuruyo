using UnityEngine;

[RequireComponent(typeof(PrototypeEnemyProjectileShooter))]
public sealed class PrototypeEnemyTurret : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private float shotInterval = 1.25f;
    [SerializeField] private bool aimAtPlayer = true;
    [SerializeField] private Vector2 fixedAimDirection = Vector2.left;
    [Min(0f)]
    [SerializeField] private float muzzleDistance = 0.8f;

    private PrototypeEnemyProjectileShooter shooter;
    private PrototypeEnemyActivation activation;
    private float nextShotTime;

    private void Awake()
    {
        shooter = GetComponent<PrototypeEnemyProjectileShooter>();
        activation = GetComponent<PrototypeEnemyActivation>();
    }

    private void Update()
    {
        if (activation != null && !activation.IsActive)
        {
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        Vector2 targetPosition;
        bool fired;
        if (aimAtPlayer)
        {
            PrototypePlayerController player = FindFirstObjectByType<PrototypePlayerController>();
            if (player == null)
            {
                return;
            }

            targetPosition = player.transform.position;
            fired = shooter.FireAtFromCenter(targetPosition, muzzleDistance);
        }
        else
        {
            targetPosition = (Vector2)transform.position + fixedAimDirection.normalized;
            fired = shooter.FireAt(targetPosition);
        }

        if (fired)
        {
            nextShotTime = Time.time + Mathf.Max(0.05f, shotInterval);
        }
    }
}
