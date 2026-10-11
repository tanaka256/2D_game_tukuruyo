using UnityEngine;

/// <summary>
/// Builds a small, code-only test room when the prototype scene starts.
/// The generated objects are intentionally plain so they can later be replaced
/// by authored sprites, prefabs, and hand-tuned level geometry.
/// </summary>
public sealed class PrototypeLevelBootstrap : MonoBehaviour
{
    private static Sprite squareSprite;

    // Deliberately not auto-run. Build this temporary room explicitly when needed;
    // otherwise it would appear in every authored scene that is opened.
    private static void CreatePrototype()
    {
        if (FindFirstObjectByType<PrototypeLevelBootstrap>() != null)
        {
            return;
        }

        GameObject rootObject = new("PrototypeLevel");
        PrototypeLevelBootstrap bootstrap = rootObject.AddComponent<PrototypeLevelBootstrap>();
        bootstrap.BuildLevel();
    }

    private void BuildLevel()
    {
        ConfigureCamera();

        CreatePlatform("Ground", new Vector2(0f, -2f), new Vector2(24f, 1f), new Color(0.24f, 0.28f, 0.32f));
        CreatePlatform("LeftPlatform", new Vector2(-5f, 0.25f), new Vector2(4f, 0.5f), new Color(0.33f, 0.38f, 0.43f));
        CreatePlatform("MiddlePlatform", new Vector2(0f, 2f), new Vector2(4f, 0.5f), new Color(0.33f, 0.38f, 0.43f));
        CreatePlatform("RightPlatform", new Vector2(5f, 3.75f), new Vector2(4f, 0.5f), new Color(0.33f, 0.38f, 0.43f));
        CreatePlatform("SmallPlatform", new Vector2(9f, 0.8f), new Vector2(3f, 0.5f), new Color(0.33f, 0.38f, 0.43f));

        CreatePlayer();
    }

    private void ConfigureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.transform.position = new Vector3(0f, 1.25f, -10f);
        camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void CreatePlayer()
    {
        GameObject player = CreateSquare("Player", new Vector2(0f, 1f), new Vector2(0.8f, 1.4f), new Color(0.2f, 0.72f, 1f));
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        player.AddComponent<PrototypePlayerController>();
        player.AddComponent<PrototypeCameraFollow>();
    }

    private void CreatePlatform(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject platform = CreateSquare(objectName, position, size, color);
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
    }

    private GameObject CreateSquare(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject square = new(objectName);
        square.transform.SetParent(transform);
        square.transform.position = position;
        square.transform.localScale = size;

        SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSquareSprite();
        renderer.color = color;
        return square;
    }

    private static Sprite GetSquareSprite()
    {
        if (squareSprite == null)
        {
            squareSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            squareSprite.name = "PrototypeWhiteSquare";
        }

        return squareSprite;
    }
}
