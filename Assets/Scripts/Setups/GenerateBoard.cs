using TMPro;
using UnityEngine;

public class GenerateBoard : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject pinkTilePrefab;
    public GameObject blueTilePrefab;
    public GameObject finishTilePrefab;

    [Header("Board Settings")]
    public int rows = 10;
    public int columns = 10;
    public float tileSize = 32f; // Sprite size in pixels
    public int pixelsPerUnit = 100; // Match your sprite’s import setting

    [Header("Text Settings")]
    public TMP_FontAsset numberFont;
    public int fontSize = 10;
    public Color textColor = Color.black;
    public Color outlineColor = Color.black;
    [Range(0f, 1f)] public float outlineWidth = 0.2f;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        float unitSize = tileSize / pixelsPerUnit;
        int tileNumber = 1;

        for (int y = 0; y < rows; y++)
        {
            bool reverseRow = (y % 2 == 1); // Snake pattern: every other row is reversed

            for (int x = 0; x < columns; x++)
            {
                int posX = reverseRow ? (columns - 1 - x) : x;

                // Decide color based on actual placed column (posX) and row (y)
                bool isPink = ((posX + y) % 2 == 0);

                GameObject tilePrefab;
                if (y == rows - 1 && x == columns - 1)
                    tilePrefab = finishTilePrefab;
                else
                    tilePrefab = isPink ? pinkTilePrefab : blueTilePrefab;

                // Instantiate tile at the visual position
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.position = new Vector2(posX * unitSize, y * unitSize);

                // Only add TMP when it's NOT tile #100 and NOT the finish tile
                if (tileNumber != 100 && tilePrefab != finishTilePrefab)
                {
                    GameObject textObj = new GameObject("TileNumber");
                    textObj.transform.SetParent(tile.transform, false);
                    var text = textObj.AddComponent<TextMeshPro>();
                    text.text = tileNumber.ToString();
                    text.font = numberFont;
                    text.fontSize = fontSize;
                    text.color = textColor;
                    text.alignment = TextAlignmentOptions.Center;
                    text.rectTransform.localPosition = Vector3.zero;

                    // Outline
                    text.outlineColor = outlineColor;
                    text.outlineWidth = outlineWidth;
                }

                tileNumber++;
            }
        }

        // Center the board
        float width = columns * unitSize;
        float height = rows * unitSize;
        transform.position = new Vector2(-width / 2 + unitSize / 2, -height / 2 + unitSize / 2);
    }



}

