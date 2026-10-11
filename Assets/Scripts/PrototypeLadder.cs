using UnityEngine;

/// <summary>
/// Trigger volume used by PrototypePlayerController for ladder climbing.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class PrototypeLadder : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }
}
