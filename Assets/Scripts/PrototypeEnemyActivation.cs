using UnityEngine;

/// <summary>
/// Keeps an enemy dormant until its bounds enter the camera view.
/// </summary>
public sealed class PrototypeEnemyActivation : MonoBehaviour
{
    [SerializeField] private bool requireVisibleBeforeStarting = true;
    [SerializeField] private float viewportPadding = 0.05f;
    [SerializeField] private bool activateOnlyOnce = true;

    private bool hasActivated;

    public bool IsActive => !requireVisibleBeforeStarting || hasActivated;

    private void Awake()
    {
        hasActivated = !requireVisibleBeforeStarting;
    }

    private void Update()
    {
        if (hasActivated || !requireVisibleBeforeStarting)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector3 viewportPosition = camera.WorldToViewportPoint(transform.position);
        bool visible = viewportPosition.z > 0f
            && viewportPosition.x >= -viewportPadding
            && viewportPosition.x <= 1f + viewportPadding
            && viewportPosition.y >= -viewportPadding
            && viewportPosition.y <= 1f + viewportPadding;

        if (visible)
        {
            hasActivated = true;
        }
    }

    public void Activate()
    {
        hasActivated = true;
    }

    public void Deactivate()
    {
        if (!activateOnlyOnce)
        {
            hasActivated = false;
        }
    }
}
