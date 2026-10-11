using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Repeats a background sprite around the camera so scrolling never exposes an empty area.
/// </summary>
public sealed class PrototypeLoopingBackground : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Vector2 tileSize = new(20f, 12f);
    [SerializeField] private Color tint = Color.white;
    [SerializeField] private int sortingOrder = -100;

    [Header("Looping")]
    [SerializeField] private bool loopHorizontal = true;
    [SerializeField] private bool loopVertical;
    [Range(0f, 1f)]
    [SerializeField] private float followFactor = 1f;

    private readonly List<SpriteRenderer> tiles = new();
    private Camera targetCamera;

    public Sprite BackgroundSprite => backgroundSprite;
    public Vector2 TileSize => tileSize;

    private void Awake()
    {
        EnsureTiles();
    }

    private void Start()
    {
        targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null || backgroundSprite == null)
        {
            return;
        }

        tileSize.x = Mathf.Max(0.1f, tileSize.x);
        tileSize.y = Mathf.Max(0.1f, tileSize.y);
        transform.position = new Vector3(
            targetCamera.transform.position.x * followFactor,
            targetCamera.transform.position.y * followFactor,
            transform.position.z);

        float localCameraX = targetCamera.transform.position.x - transform.position.x;
        float localCameraY = targetCamera.transform.position.y - transform.position.y;
        float centerX = loopHorizontal ? Mathf.Floor(localCameraX / tileSize.x) * tileSize.x : 0f;
        float centerY = loopVertical ? Mathf.Floor(localCameraY / tileSize.y) * tileSize.y : 0f;

        int index = 0;
        int columns = loopHorizontal ? 3 : 1;
        int rows = loopVertical ? 3 : 1;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (index >= tiles.Count)
                {
                    return;
                }

                tiles[index].transform.localPosition = new Vector3(
                    centerX + (x - (columns - 1) * 0.5f) * tileSize.x,
                    centerY + (y - (rows - 1) * 0.5f) * tileSize.y,
                    0f);
                index++;
            }
        }
    }

    public void BuildPreviewTiles()
    {
        EnsureTiles(true);
    }

    private void EnsureTiles(bool rebuild = false)
    {
        if (backgroundSprite == null)
        {
            return;
        }

        if (rebuild)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            tiles.Clear();
        }

        if (tiles.Count == 0)
        {
            SpriteRenderer[] existingTiles = GetComponentsInChildren<SpriteRenderer>(true);
            tiles.AddRange(existingTiles);
        }

        int requiredCount = (loopHorizontal ? 3 : 1) * (loopVertical ? 3 : 1);
        while (tiles.Count < requiredCount)
        {
            GameObject tileObject = new("BackgroundTile");
            tileObject.transform.SetParent(transform, false);
            tiles.Add(tileObject.AddComponent<SpriteRenderer>());
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            SpriteRenderer renderer = tiles[i];
            renderer.sprite = backgroundSprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;
            renderer.transform.localScale = new Vector3(
                tileSize.x / Mathf.Max(0.01f, backgroundSprite.bounds.size.x),
                tileSize.y / Mathf.Max(0.01f, backgroundSprite.bounds.size.y),
                1f);
        }
    }
}
