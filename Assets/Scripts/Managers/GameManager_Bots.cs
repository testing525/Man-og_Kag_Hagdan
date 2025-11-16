using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class GameManager_Bots : MonoBehaviour
{
    [Header("References")]
    public Dice diceUI;
    public BoardManager boardManager;
    public ScoreManager scoreManager;
    public EventScript eventScript; 
    public PlayerInventory inventoryUI;
    public PlayerState playerState;
    public StunSelectionUI stunSelectionUI;
    public BombPlacement bombPlacement;

    [Header("Players")]
    public GameObject[] players;
    public PlayerProfile[] playerProfiles;
    public PlayerStates playerStates;

    [Header("Player Types")]
    public bool[] isBot; // length = 4, true if player is bot



    [Header("Player Settings")]
    public float yOffset = 0.2f;
    public float moveSpeed = 10f;
    public float ladderSnakeSpeed = 5f;
    public float stepDelay = 0.1f;

    public int[] currentTiles;
    public int currentPlayerIndex = 0;
    private bool isMoving = false;

    public int[] ladderTiles = new int[] { 4, 9, 28, 39, 77 };
    public int[] snakeTiles = new int[] { 26, 64, 90, 81, 94 };




    void Start()
    {
        for (int i = 0; i < players.Length; i++)
        {
            PlacePlayerAtStart(i);

            // Assign PlayerProfile to PlayerMarker
            PlayerMarker marker = players[i].GetComponent<PlayerMarker>();
            if (marker != null)
            {
                marker.playerProfile = playerProfiles[i];
            }
        }

        if (players.Length != 4 || playerProfiles.Length != 4)
        {
            Debug.LogError("⚠️ Assign exactly 4 players and 4 PlayerProfiles!");
            return;
        }

        if (scoreManager != null)
        {
            scoreManager.SetPlayerProfiles(playerProfiles);
        }

        playerStates = GetComponent<PlayerStates>();

        foreach (var profile in playerProfiles)
        {
            playerStates.RegisterPlayer(profile);
        }


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
        if (isMoving || eventScript.currentState == State.PlayerCheckingInventory)
            return; // pause dice rolling if player is checking inventory

        Debug.Log($"🎲 Player {currentPlayerIndex + 1} rolled a {rollValue}");
        int targetTile = currentTiles[currentPlayerIndex] + rollValue;
        if (targetTile > 100) targetTile = 100;

        StartCoroutine(MovePlayerStepByStep(currentPlayerIndex, targetTile));
    }

    private bool IsBotTurn()
    {
        return isBot[currentPlayerIndex];
    }



    public IEnumerator MovePlayerStepByStep(int playerIndex, int targetTile)
    {
        isMoving = true;

        PlayerProfile profile = playerProfiles[playerIndex];

        // Step-by-step movement
        while (currentTiles[playerIndex] < targetTile)
        {
            currentTiles[playerIndex]++;
            Vector3 nextPos = boardManager.GetTilePosition(currentTiles[playerIndex]) + new Vector3(0, yOffset, 0);
            yield return StartCoroutine(MoveToPosition(players[playerIndex], nextPos, moveSpeed));

            int points = 10;
            if (playerStates.GetState(profile) == PlayerState.PointsMultiplier)
            {
                points += 5;
            }

            profile.AddPoints(points);
            UpdateScoreUI();

            yield return new WaitForSeconds(stepDelay);
        }

        // Ladder check
        int ladderTile = CheckForLadder(currentTiles[playerIndex]);
        if (ladderTile != currentTiles[playerIndex])
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(MoveSmoothLerp(players[playerIndex], ladderTile, playerIndex));
            currentTiles[playerIndex] = ladderTile;

            // Award points (consider PointsMultiplier)
            int points = 10;
            if (playerStates.GetState(profile) == PlayerState.PointsMultiplier)
                points += 5;

            profile.AddPoints(points);
            UpdateScoreUI();
        }

        // Snake check
        int snakeTile = CheckForSnake(currentTiles[playerIndex]);
        if (snakeTile != currentTiles[playerIndex])
        {
            // Check Anti Snake Spray
            if (profile.ownedItems.Contains("Anti Snake Spray"))
            {
                Debug.Log($"🧴 {profile.playerName} avoided a snake at tile {currentTiles[playerIndex]} using Anti Snake Spray!");
                profile.ownedItems.Remove("Anti Snake Spray"); // consume
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(MoveSmoothLerp(players[playerIndex], snakeTile, playerIndex));
                currentTiles[playerIndex] = snakeTile;

                // Award points for moving
                int points = 10;
                if (playerStates.GetState(profile) == PlayerState.PointsMultiplier)
                    points += 5;

                profile.AddPoints(points);
                UpdateScoreUI();
            }
        }


        Debug.Log($"📍 Player {playerIndex + 1} reached tile {currentTiles[playerIndex]}");

        isMoving = false;

        // Inventory check
        PlayerProfile currentProfile = playerProfiles[playerIndex];
        if (currentProfile.ownedItems.Count > 0 && inventoryUI != null
            && eventScript.currentState != State.PlayerIsUsingItem)
        {
            Debug.Log($"👜 Player {playerIndex + 1} has items. Showing inventory...");
            inventoryUI.ShowInventory(currentProfile);
            eventScript.currentState = State.PlayerCheckingInventory;

            isMoving = false;
            yield break; // pause until player uses item or cancels
        }


        // No items: proceed automatically
        ProceedToNextPlayer();
    }

    private IEnumerator BotTurn()
    {
        yield return new WaitForSeconds(0.5f); // small delay for realism

        // Roll dice with animation and callback
        diceUI.RollForBot((rollValue) =>
        {
            Debug.Log($"🤖 {playerProfiles[currentPlayerIndex].playerName} rolled {rollValue}");
            int targetTile = currentTiles[currentPlayerIndex] + rollValue;
            if (targetTile > 100) targetTile = 100;

            StartCoroutine(MovePlayerStepByStep(currentPlayerIndex, targetTile));
        });
    }


    public IEnumerator MovePlayerBackwards(int playerIndex, int tilesToMoveBack)
    {
        int startTile = currentTiles[playerIndex];
        int targetTile = startTile - tilesToMoveBack;
        if (targetTile < 1) targetTile = 1; // clamp to start

        Debug.Log($"⏪ Moving {playerProfiles[playerIndex].playerName} back from tile {startTile} to {targetTile}");

        isMoving = true;

        while (currentTiles[playerIndex] > targetTile)
        {
            currentTiles[playerIndex]--;
            Vector3 nextPos = boardManager.GetTilePosition(currentTiles[playerIndex]) + new Vector3(0, yOffset, 0);
            yield return StartCoroutine(MoveToPosition(players[playerIndex], nextPos, moveSpeed));
            yield return new WaitForSeconds(stepDelay);            
        }

        isMoving = false;
    }




    public void ProceedToNextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playerProfiles.Length;
        if (currentPlayerIndex == 0)   // Player1’s turn → new round
        {
            eventScript.OnNewRound();
        }

        PlayerProfile next = playerProfiles[currentPlayerIndex];

        // Check player state
        if (playerStates.ProcessStateAtTurnStart(next))
        {
            Debug.Log($"⛔ {next.playerName}'s turn skipped (stunned).");
            ProceedToNextPlayer(); // skip and move to next
            return;
        }

        eventScript.currentState = State.Normal;

        Debug.Log($"🎲 It is now {next.playerName}'s turn.");

        if (IsBotTurn())
        {
            // start bot roll
            StartCoroutine(BotTurn());
        }

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

    private int CheckForLadder(int tile)
    {
        if (tile == 4) return 17;
        if (tile == 9) return 30;
        if (tile == 28) return 55;
        if (tile == 39) return 58;
        if (tile == 77) return 84;
        if (tile == 26) return 5;
        if (tile == 64) return 45;
        if (tile == 90) return 32;
        if (tile == 81) return 62;
        if (tile == 94) return 73;

        return tile;
    }
    private int CheckForSnake(int tile)
    {
        if (tile == 26) return 5;
        if (tile == 64) return 45;
        if (tile == 90) return 32;
        if (tile == 81) return 62;
        if (tile == 94) return 73;

        return tile;
    }


    // ────────────────────────────────
    // ITEMS
    // ────────────────────────────────
    public void OnItemUsed(PlayerProfile player, string itemName)
    {
        eventScript.currentState = State.PlayerIsUsingItem;

        Debug.Log($"🎁 Item used: {itemName} by {player.playerName}");
        inventoryUI.gameObject.SetActive(false);

        if (player.ownedItems.Contains(itemName))
        {
            player.ownedItems.Remove(itemName);
            Debug.Log($"🗑️ {itemName} consumed and removed from {player.playerName}'s inventory");
        }

        if (itemName == "Pogo Stick")
        {
            // Pogo movement effect
            StartCoroutine(UsePogo(player, 4));
        }
        else
        {
            // Apply other item effect here (example: Shield)
            ApplyItemEffect(player, itemName);

            // After effect is applied, proceed to next player
            StartCoroutine(ProceedAfterItemEffect());
        }
    }


    private IEnumerator ProceedAfterItemEffect()
    {
        // Optional: wait a tiny bit for visual effect or animation
        yield return new WaitForSeconds(0.2f);

        if (eventScript.currentState != State.PlayerIsPlacingItem)
        {
            eventScript.currentState = State.Normal;
            ProceedToNextPlayer();
        }
    }

    private void ApplyItemEffect(PlayerProfile player, string itemName)
    {
        switch (itemName)
        {
            case "Shield":
                playerStates.SetShielded(player);
                Debug.Log($"🛡 Shield applied for {player.playerName}");
                break;
            case "ExtraPoints":
                player.AddPoints(20);
                UpdateScoreUI();
                break;
            case "Stun Gun":
                stunSelectionUI.Show(player);
                break;
            case "Bomb":
                bombPlacement.StartPlacingBomb(player);
                break;
            case "Points  Multiplier":
                playerStates.SetPointsMultiplier(player);
                break;

                // Add more items here
        }
    }



    private IEnumerator UsePogo(PlayerProfile player, int tilesToMove)
    {
        int playerIndex = System.Array.IndexOf(playerProfiles, player);
        if (playerIndex < 0)
        {
            Debug.LogWarning("⚠️ Player not found in profiles!");
            yield break;
        }

        int targetTile = currentTiles[playerIndex] + tilesToMove;
        if (targetTile > 100) targetTile = 100;
        if (targetTile < 1) targetTile = 1; // just in case of negative tiles

        Debug.Log($"🦘 Pogo Stick activated! Player {playerIndex + 1} jumps to {targetTile}");

        // Reuse MovePlayerStepByStep
        yield return StartCoroutine(MovePlayerStepByStep(playerIndex, targetTile));
        eventScript.currentState = State.Normal;

    }





}
