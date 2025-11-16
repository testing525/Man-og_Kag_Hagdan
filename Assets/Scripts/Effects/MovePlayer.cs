using UnityEngine;
using System.Collections;

public class MovePlayer : MonoBehaviour
{
    public BoardManager boardManager;
    public float yOffset = 0.2f;
    public float moveSpeed = 10f;
    public float stepDelay = 0.1f;

    public IEnumerator Move(GameObject playerObj, int startTile, int endTile, System.Action<int> updateTile)
    {
        int direction = endTile > startTile ? 1 : -1;

        int tile = startTile;

        while (tile != endTile)
        {
            tile += direction;
            updateTile(tile);

            Vector3 nextPos = boardManager.GetTilePosition(tile) + new Vector3(0, yOffset, 0);

            // Smooth step
            while (Vector3.Distance(playerObj.transform.position, nextPos) > 0.01f)
            {
                playerObj.transform.position =
                    Vector3.MoveTowards(playerObj.transform.position, nextPos, Time.deltaTime * moveSpeed);
                yield return null;
            }

            playerObj.transform.position = nextPos;

            yield return new WaitForSeconds(stepDelay);
        }
    }
}
