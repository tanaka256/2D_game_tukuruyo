using UnityEngine;

[RequireComponent(typeof(PrototypeEnemyProjectileShooter))]
public sealed class PrototypeEnemyOmniTurret : MonoBehaviour
{
    [Header("Detection")]
    [Min(0.1f)]
    [SerializeField] private float detectionRange = 8f;
    [Min(0f)]
    [SerializeField] private float muzzleDistance = 0.8f;

    [Header("Shooting")]
    [SerializeField] private float shotInterval = 1.25f;
    [SerializeField] private bool showDetectionGizmo = true;
    [SerializeField] private Color detectionGizmoColor = new(1f, 0.35f, 0.2f, 0.7f);

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

        PrototypePlayerController player = FindFirstObjectByType<PrototypePlayerController>();
        if (player == null || Vector2.Distance(transform.position, player.transform.position) > detectionRange)
        {
            return;
        }

        if (Time.time >= nextShotTime
            && shooter.FireAtFromCenter(player.transform.position, muzzleDistance))
        {
            nextShotTime = Time.time + Mathf.Max(0.05f, shotInterval);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDetectionGizmo)
        {
            return;
        }

        Gizmos.color = detectionGizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
