using UnityEngine;

public class Tile : MonoBehaviour
{
    public int tileNumber;

    void Awake()
    {
        if (!int.TryParse(gameObject.name, out tileNumber))
        {
            Debug.LogWarning($"Tile name '{gameObject.name}' is not a valid number!");
        }
    }
}
