using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Dice diceUI;
    public BoardManager boardManager;
    public ScoreManager scoreManager;
    public EventScript eventScript; // 👈 Add this

    [Header("Players")]
    public GameObject[] players;
    public PlayerProfile[] playerProfiles;

    [Header("Player Settings")]
    public float yOffset = 0.2f;
    public float moveSpeed = 10f;
    public float ladderSnakeSpeed = 5f;
    public float stepDelay = 0.1f;

    private int[] currentTiles;
    private int currentPlayerIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        if (players.Length != 4 || playerProfiles.Length != 4)
        {
            Debug.LogError("⚠️ Assign exactly 4 players and 4 PlayerProfiles!");
            return;
        }

        if (scoreManager != null)
            scoreManager.SetPlayerProfiles(playerProfiles);

        diceUI.OnDiceRolled += HandleDiceResult;

        currentTiles = new int[4];
        for (int i = 0; i < 4; i++)
        {
            currentTiles[i] = 0;
            PlacePlayerAtStart(i);
        }

        scoreManager.UpdateScoreUI();
        Debug.Log($"Player {currentPlayerIndex + 1}'s turn!");
    }

    void OnDestroy()
    {
        if (diceUI != null)
            diceUI.OnDiceRolled -= HandleDiceResult;
    }

    private void HandleDiceResult(int rollValue)
    {
        if (isMoving) return;

        Debug.Log($"🎲 Player {currentPlayerIndex + 1} rolled a {rollValue}");
        int targetTile = currentTiles[currentPlayerIndex] + rollValue;
        if (targetTile > 100) targetTile = 100;

        StartCoroutine(MovePlayerStepByStep(currentPlayerIndex, targetTile));
    }

    private IEnumerator MovePlayerStepByStep(int playerIndex, int targetTile)
    {
        isMoving = true;

        while (currentTiles[playerIndex] < targetTile)
        {
            currentTiles[playerIndex]++;
            Vector3 nextPos = boardManager.GetTilePosition(currentTiles[playerIndex])
                  + new Vector3(0, yOffset, 0);
            yield return StartCoroutine(MoveToPosition(players[playerIndex], nextPos, moveSpeed));

            playerProfiles[playerIndex].AddPoints(10);
            UpdateScoreUI();
            yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log($"📍 Player {playerIndex + 1} reached tile {currentTiles[playerIndex]}");

        int destinationTile = CheckForSnakeOrLadder(currentTiles[playerIndex]);
        if (destinationTile != currentTiles[playerIndex])
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(MoveSmoothLerp(players[playerIndex], destinationTile, playerIndex));
            currentTiles[playerIndex] = destinationTile;
            playerProfiles[playerIndex].AddPoints(10);
            UpdateScoreUI();
        }

        if (currentTiles[playerIndex] == 100)
            Debug.Log($"🏁 Player {playerIndex + 1} reached the finish line!");

        // Next turn
        currentPlayerIndex = (currentPlayerIndex + 1) % 4;

        // 🟢 Notify the EventScript every time it's Player 1’s turn
        if (currentPlayerIndex == 0 && eventScript != null)
        {
            eventScript.OnNewRound();
        }

        Debug.Log($"Player {currentPlayerIndex + 1}'s turn!");
        UpdateScoreUI();
        isMoving = false;
    }

    private void UpdateScoreUI()
    {
        if (scoreManager != null)
            scoreManager.UpdateScoreUI();
    }

    private IEnumerator MoveToPosition(GameObject player, Vector3 target, float speed)
    {
        while (Vector3.Distance(player.transform.position, target) > 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, target, Time.deltaTime * speed);
            yield return null;
        }
        player.transform.position = target;
    }

    private IEnumerator MoveSmoothLerp(GameObject player, int destinationTile, int playerIndex)
    {
        Vector3 start = player.transform.position;
        Vector3 end = boardManager.GetTilePosition(destinationTile) + new Vector3(0, yOffset, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * ladderSnakeSpeed;
            player.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        player.transform.position = end;
    }

    private void PlacePlayerAtStart(int playerIndex)
    {
        Vector3 startTilePos = boardManager.GetTilePosition(1);
        players[playerIndex].transform.position = startTilePos + new Vector3(0, yOffset, 0);
    }

    private int CheckForSnakeOrLadder(int tile)
    {
        if (tile == 4) return 17;
        if (tile == 9) return 30;
        if (tile == 28) return 55;
        if (tile == 39) return 58;
        if (tile == 77) return 84;
        if (tile == 26) return 9;
        if (tile == 64) return 45;
        if (tile == 90) return 32;
        if (tile == 81) return 62;
        if (tile == 94) return 73;

        return tile;
    }
}
