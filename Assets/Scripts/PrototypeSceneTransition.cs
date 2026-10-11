using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Carries a door destination across a scene load and places the player at the
/// matching door's arrival point in the destination scene.
/// </summary>
public static class PrototypeSceneTransition
{
    private static bool initialized;
    private static bool hasPendingDestination;
    private static string pendingDoorId;
    private static Vector3 pendingFallbackPosition;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void LoadScene(string sceneName, string destinationDoorId, Vector3 fallbackPosition)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("PrototypeSceneTransition: destination scene is empty.");
            return;
        }

        hasPendingDestination = true;
        pendingDoorId = destinationDoorId;
        pendingFallbackPosition = fallbackPosition;
        SceneManager.LoadScene(sceneName);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingDestination)
        {
            return;
        }

        PrototypePlayerController player = Object.FindFirstObjectByType<PrototypePlayerController>();
        if (player == null)
        {
            hasPendingDestination = false;
            Debug.LogWarning("PrototypeSceneTransition: no player was found in the destination scene.");
            return;
        }

        Vector3 spawnPosition = pendingFallbackPosition;
        if (!string.IsNullOrWhiteSpace(pendingDoorId))
        {
            PrototypeSceneDoor[] doors = Object.FindObjectsByType<PrototypeSceneDoor>(FindObjectsSortMode.None);
            foreach (PrototypeSceneDoor door in doors)
            {
                if (door.DoorId == pendingDoorId)
                {
                    spawnPosition = door.ArrivalPosition;
                    break;
                }
            }
        }

        player.transform.position = spawnPosition;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        hasPendingDestination = false;
    }
}
