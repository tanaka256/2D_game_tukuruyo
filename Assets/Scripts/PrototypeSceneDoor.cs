using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Trigger door that loads another scene when the player is inside and presses up.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class PrototypeSceneDoor : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string doorId = "Door";
    [SerializeField] private string destinationScene;
    [SerializeField] private string destinationDoorId;
    [SerializeField] private Vector3 fallbackSpawnPosition;

    [Header("Arrival")]
    [SerializeField] private Transform arrivalPoint;

    [Header("Editor Visualization")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = new(0.2f, 0.9f, 1f, 0.8f);

    private bool playerInside;
    private bool transitioning;

    public string DoorId => doorId;
    public Vector3 ArrivalPosition => arrivalPoint != null ? arrivalPoint.position : transform.position;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PrototypePlayerController>() != null)
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PrototypePlayerController>() != null)
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (!playerInside || transitioning || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            transitioning = true;
            PrototypeSceneTransition.LoadScene(destinationScene, destinationDoorId, fallbackSpawnPosition);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        if (arrivalPoint != null)
        {
            Gizmos.DrawLine(transform.position, arrivalPoint.position);
            Gizmos.DrawWireSphere(arrivalPoint.position, 0.15f);
        }
    }
}
