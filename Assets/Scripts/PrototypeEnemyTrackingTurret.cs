using UnityEngine;

[RequireComponent(typeof(PrototypeEnemyProjectileShooter))]
public sealed class PrototypeEnemyTrackingTurret : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private float shotInterval = 1.25f;
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

        PrototypePlayerController player = FindFirstObjectByType<PrototypePlayerController>();
        if (player == null)
        {
            return;
        }

        if (Time.time >= nextShotTime
            && shooter.FireAtFromCenter(player.transform.position, muzzleDistance))
        {
            nextShotTime = Time.time + Mathf.Max(0.05f, shotInterval);
        }
    }
}
