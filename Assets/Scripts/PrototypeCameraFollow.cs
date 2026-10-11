using UnityEngine;

/// <summary>
/// Optional camera follow component included in the Player prefab.
/// </summary>
public sealed class PrototypeCameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float horizontalOffset = 0f;
    [SerializeField] private float verticalPosition = 1.25f;

    private Camera targetCamera;

    private void Start()
    {
        targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 targetPosition = new(
            transform.position.x + horizontalOffset,
            verticalPosition,
            targetCamera.transform.position.z);
        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            targetPosition,
            followSpeed * Time.deltaTime);
    }
}
