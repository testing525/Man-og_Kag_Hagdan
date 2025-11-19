using UnityEngine;

public class BombPlacement : MonoBehaviour
{
    public GameManager gameManager;
    public GameManager_Bots gameManagerBots;
    public GameObject bombPrefab;

    private PlayerProfile currentUser;
    private bool isPlacing = false;

    public void StartPlacingBomb(PlayerProfile user)
    {
        currentUser = user;
        isPlacing = true;
        Debug.Log($"💣 {user.playerName} is placing a bomb. Click a tile!");

        if (gameManager != null)
            gameManager.eventScript.currentState = State.PlayerIsPlacingItem;
        else if (gameManagerBots != null)
            gameManagerBots.eventScript.currentState = State.PlayerIsPlacingItem;
    }

    void Update()
    {
        if (!isPlacing) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    PlaceBombOnTile(tile);
                }
            }
        }
    }

    private void PlaceBombOnTile(Tile tile)
    {
        Debug.Log($"💣 Bomb placed on tile {tile.tileNumber}");

        Vector3 spawnPos = tile.transform.position;
        Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        isPlacing = false;
    }
}
