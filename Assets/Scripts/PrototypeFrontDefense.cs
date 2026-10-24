using UnityEngine;

/// <summary>
/// Blocks projectile damage that arrives from this enemy's facing direction.
/// </summary>
public sealed class PrototypeFrontDefense : MonoBehaviour
{
    [SerializeField] private bool defenseEnabled = true;
    [SerializeField] private bool showDefenseGizmo = true;
    [SerializeField] private Color defenseGizmoColor = new(0.2f, 0.8f, 1f, 0.8f);

    public bool DefenseEnabled => defenseEnabled;

    public bool BlocksAttackFrom(GameObject source)
    {
        if (!defenseEnabled || source == null)
        {
            return false;
        }

        int facingDirection = 1;
        PrototypeEnemyPatrol patrol = GetComponent<PrototypeEnemyPatrol>();
        if (patrol != null)
        {
            facingDirection = patrol.FacingDirection;
        }
        else
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.flipX)
            {
                facingDirection = -1;
            }
        }

        float incomingSide = source.transform.position.x - transform.position.x;
        return Mathf.Abs(incomingSide) > 0.01f && Mathf.Sign(incomingSide) == facingDirection;
    }

    private void OnDrawGizmos()
    {
        if (!showDefenseGizmo)
        {
            return;
        }

        PrototypeEnemyPatrol patrol = GetComponent<PrototypeEnemyPatrol>();
        int direction = patrol != null ? patrol.FacingDirection : 1;
        Gizmos.color = defenseGizmoColor;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * direction * 0.65f);
        Gizmos.DrawWireSphere(transform.position + Vector3.right * direction * 0.65f, 0.1f);
    }
}
