using UnityEngine;

public class TileSelect : MonoBehaviour
{
    [Header("Tile Settings")]
    public LayerMask tileLayerMask; // Assign the layer your tiles are on

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, tileLayerMask);

            if (hit.collider != null)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    Debug.Log($"🖱️ Tile clicked: {tile.tileNumber}");
                }
            }
        }
    }
}
