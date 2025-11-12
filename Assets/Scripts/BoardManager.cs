using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Tiles")]
    public List<GameObject> tiles = new List<GameObject>(); // Stores tile GameObjects from 1–100
    public Transform tilesParent; // Parent of all generated tiles (assign in Inspector)

    void Awake()
    {
        InitializeBoard();
    }

    /// <summary>
    /// Collects all tile GameObjects under the parent and stores them in order.
    /// </summary>
    void InitializeBoard()
    {
        tiles.Clear();

        if (tilesParent == null)
        {
            Debug.LogError("⚠️ Tiles parent not assigned in BoardManager!");
            return;
        }

        // Get all child tiles from the board
        foreach (Transform child in tilesParent)
        {
            tiles.Add(child.gameObject);
        }

        // Sort by tile number (based on the TMP text or name)
        tiles.Sort((a, b) =>
        {
            // Example: tiles named "Tile_1", "Tile_2", etc.
            // Try to get number from name
            int numA = ExtractTileNumber(a.name);
            int numB = ExtractTileNumber(b.name);
            return numA.CompareTo(numB);
        });

        Debug.Log($"✅ Board initialized with {tiles.Count} tiles.");
    }

    int ExtractTileNumber(string name)
    {
        // Try to parse any number at the end of the tile name
        string digits = "";
        foreach (char c in name)
        {
            if (char.IsDigit(c)) digits += c;
        }

        int.TryParse(digits, out int result);
        return result;
    }

    /// <summary>
    /// Returns the position of a given tile number (1–100).
    /// </summary>
    public Vector3 GetTilePosition(int tileNumber)
    {
        if (tileNumber < 1 || tileNumber > tiles.Count)
        {
            Debug.LogWarning($"❌ Invalid tile number: {tileNumber}");
            return Vector3.zero;
        }

        return tiles[tileNumber - 1].transform.position;
    }

    /// <summary>
    /// Moves a player to a tile number.
    /// </summary>
    public void MovePlayer(GameObject player, int tileNumber)
    {
        if (player == null || tileNumber < 1 || tileNumber > tiles.Count)
            return;

        player.transform.position = GetTilePosition(tileNumber);
        Debug.Log($"🎯 Player moved to tile {tileNumber}");
    }
}
