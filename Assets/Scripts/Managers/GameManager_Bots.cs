using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager_Bots : MonoBehaviour
{
    public static GameManager_Bots instance;
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

    [Header("Player Settings")]
    public float yOffset = 0.2f;
    public float moveSpeed = 10f;
    public float ladderSnakeSpeed = 5f;
    public float stepDelay = 0.1f;

    public int currentPlayerIndex = 0;
    private bool isMoving = false;

    public int[] ladderTiles = new int[] { 4, 9, 28, 39, 77 };
    public int[] snakeTiles = new int[] { 26, 64, 90, 81, 94 };


    private void Awake()
    {
        instance = this;
    }

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
        int targetTile = playerProfiles[currentPlayerIndex].currentTile + rollValue;
        if (targetTile > 100) targetTile = 100;

        StartCoroutine(MovePlayerStepByStep(currentPlayerIndex, targetTile));
    }

    private bool IsBotTurn()
    {
        return playerProfiles[currentPlayerIndex].isBot;
    }




    public IEnumerator MovePlayerStepByStep(int playerIndex, int targetTile)
    {
        isMoving = true;

        PlayerProfile profile = playerProfiles[playerIndex];

        // Step-by-step movement
        while (playerProfiles[playerIndex].currentTile < targetTile)
        {
            playerProfiles[playerIndex].currentTile++;
            Vector3 nextPos = boardManager.GetTilePosition(playerProfiles[playerIndex].currentTile) +
                              new Vector3(0, yOffset, 0);

            yield return StartCoroutine(MoveToPosition(players[playerIndex], nextPos, moveSpeed));

            int points = 10;
            if (playerStates.GetState(profile) == PlayerState.PointsMultiplier)
                points += 5;

            profile.AddPoints(points);
            UpdateScoreUI();

            yield return new WaitForSeconds(stepDelay);
        }

        // WIN / CROWN CHECK
        if (playerProfiles[playerIndex].currentTile == 100)
        {
            Debug.Log($"👑 {playerProfiles[playerIndex].playerName} reached tile 100!");

            // Add 5000 points
            playerProfiles[playerIndex].AddPoints(5000);
            UpdateScoreUI();

            // Add crown
            playerProfiles[playerIndex].crowns++;
            Debug.Log($"👑 Crown awarded! {playerProfiles[playerIndex].playerName} now has {playerProfiles[playerIndex].crowns} crown(s)");

            // Did they win the game?
            if (playerProfiles[playerIndex].crowns >= 2)
            {
                Debug.Log($"🏆 {playerProfiles[playerIndex].playerName} WINS THE GAME!");
                yield break; // STOP ALL MOVEMENT / TURNS
            }

            // If not yet winner, reset to tile 1
            playerProfiles[playerIndex].currentTile = 1;

            // Snap them to tile 1
            Vector3 resetPos = boardManager.GetTilePosition(1) + new Vector3(0, yOffset, 0);
            players[playerIndex].transform.position = resetPos;

            Debug.Log($"🔄 {playerProfiles[playerIndex].playerName} returns to tile 1!");

            isMoving = false;

            // Continue turn flow
            yield return StartCoroutine(HandlePostMoveActions(playerIndex));
            yield break;
        }

        // Ladder check
        int ladderTile = CheckForLadder(playerProfiles[playerIndex].currentTile);
        if (ladderTile != playerProfiles[playerIndex].currentTile)
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(MoveSmoothLerp(players[playerIndex], ladderTile, playerIndex));
            playerProfiles[playerIndex].currentTile = ladderTile;

            int points = 10;
            if (playerStates.GetState(profile) == PlayerState.PointsMultiplier)
                points += 5;

            profile.AddPoints(points);
            UpdateScoreUI();
        }

        // Snake check
        int snakeTile = CheckForSnake(playerProfiles[playerIndex].currentTile);
        if (snakeTile != playerProfiles[playerIndex].currentTile)
        {
            if (profile.ownedItems.Contains("Anti Snake Spray"))
            {
                Debug.Log($"🧴 {profile.playerName} avoided a snake!");
                profile.ownedItems.Remove("Anti Snake Spray");
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(MoveSmoothLerp(players[playerIndex], snakeTile, playerIndex));
                playerProfiles[playerIndex].currentTile = snakeTile;

                int points = 10;
                if (playerStates.GetState(profile) == PlayerState.PointsMultiplier)
                    points += 5;

                profile.AddPoints(points);
                UpdateScoreUI();
            }
        }

        Debug.Log($"📍 Player {playerIndex + 1} reached tile {playerProfiles[playerIndex].currentTile}");

        isMoving = false;

        // call post-movement logic after
        yield return StartCoroutine(HandlePostMoveActions(playerIndex));
    }

    private IEnumerator HandlePostMoveActions(int playerIndex)
    {
        PlayerProfile profile = playerProfiles[playerIndex];

        // HUMAN PLAYER
        if (!profile.isBot && profile.ownedItems.Count > 0)
        {
            Debug.Log($"👜 Player {playerIndex + 1} has items. Showing inventory...");
            eventScript.currentState = State.PlayerCheckingInventory;
            inventoryUI.ShowInventory(profile);
            yield break;
        }
        // BOT PLAYER
        else if (profile.isBot && profile.ownedItems.Count > 0)
        {
            Debug.Log("🤖 Bot deciding to use an item...");
            eventScript.currentState = State.PlayerCheckingInventory;

            bool usedItem = false;

            yield return StartCoroutine(
                BotDecisionSystem.Instance.DecideUseItem(
                    profile,
                    (chosenItem) =>
                    {
                        if (!string.IsNullOrEmpty(chosenItem))
                        {
                            Debug.Log($"🤖 Bot uses {chosenItem}");
                            OnItemUsed(profile, chosenItem);
                            usedItem = true;
                        }
                        else
                        {
                            Debug.Log("🤖 Bot decided NOT to use any item.");
                            ProceedToNextPlayer();
  
                        }
                    })
            );

            // If an item was used (example pogo stick), wait for movement to fully finish
            if (usedItem)
            {
                while (isMoving)
                    yield return null;

                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            ProceedToNextPlayer();
        }

    }


    private IEnumerator BotTurn()
    {
        yield return new WaitForSeconds(0.5f); // small delay for realism

        // Roll dice with animation and callback
        diceUI.RollForBot((rollValue) =>
        {
            Debug.Log($"🤖 {playerProfiles[currentPlayerIndex].playerName} rolled {rollValue}");
            int targetTile = playerProfiles[currentPlayerIndex].currentTile + rollValue;
            if (targetTile > 100) targetTile = 100;

            StartCoroutine(MovePlayerStepByStep(currentPlayerIndex, targetTile));
        });
    }


    public IEnumerator MovePlayerBackwards(int playerIndex, int tilesToMoveBack)
    {
        int startTile = playerProfiles[playerIndex].currentTile;
        int targetTile = startTile - tilesToMoveBack;
        if (targetTile < 1) targetTile = 1; // clamp to start

        Debug.Log($"⏪ Moving {playerProfiles[playerIndex].playerName} back from tile {startTile} to {targetTile}");

        isMoving = true;

        while (playerProfiles[playerIndex].currentTile > targetTile)
        {
            playerProfiles[playerIndex].currentTile--;
            Vector3 nextPos = boardManager.GetTilePosition(playerProfiles[playerIndex].currentTile) + new Vector3(0, yOffset, 0);
            yield return StartCoroutine(MoveToPosition(players[playerIndex], nextPos, moveSpeed));
            yield return new WaitForSeconds(stepDelay);            
        }

        isMoving = false;
    }

    public void RegisterBombPushback(int playerIndex, int moveBack)
    {
        StartCoroutine(HandleBombPushback(playerIndex, moveBack));
    }

    private IEnumerator HandleBombPushback(int playerIndex, int moveBack)
    {
        // Lock game state
        eventScript.currentState = State.PlayerIsUsingItem;

        // Move backwards
        yield return StartCoroutine(MovePlayerBackwards(playerIndex, moveBack));

        // Restore normal state
        eventScript.currentState = State.Normal;

        // Continue turn flow
        ProceedToNextPlayer();
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
        LearningManager.Instance.RecordItemUsed(itemName, player.currentTile);
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

        if (eventScript.currentState == State.Normal)
        {
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
                eventScript.currentState = State.Normal;
                break;
            case "ExtraPoints":
                player.AddPoints(20);
                eventScript.currentState = State.Normal;
                UpdateScoreUI();
                break;
            case "Stun Gun":
                HandleStunItem(player);
                break;
            case "Bomb":
                bombPlacement.StartPlacingBomb(player);
                break;
            case "Points  Multiplier":
                playerStates.SetPointsMultiplier(player);
                eventScript.currentState = State.Normal;
                break;

                // Add more items here
        }
    }


    //ITEM USAGE
    private IEnumerator UsePogo(PlayerProfile player, int tilesToMove)
    {
        int playerIndex = System.Array.IndexOf(playerProfiles, player);
        if (playerIndex < 0)
            yield break;

        int targetTile = playerProfiles[playerIndex].currentTile + tilesToMove;
        if (targetTile > 100) targetTile = 100;

        Debug.Log($"🦘 Pogo Stick: Player {playerIndex + 1} jumps to {targetTile}");

        isMoving = true;

        yield return StartCoroutine(MovePlayerStepByStep(playerIndex, targetTile));

        isMoving = false;
        eventScript.currentState = State.Normal;
    }

    private void HandleStunItem(PlayerProfile user)
    {
        // HUMAN PLAYER: show UI
        if (!user.isBot)
        {
            stunSelectionUI.Show(user);
            return; // Wait for UI to select target
        }
        else
        {
            // BOT LOGIC: pick best target
            PlayerProfile target = BotDecisionSystem.Instance.ChooseStunTarget(user);

            if (target == null)
            {
                Debug.Log("🤖 Bot could not find stun target");
                ProceedToNextPlayer();
                return;
            }

            // Shield check
            if (target.ownedItems.Contains("Shield"))
            {
                Debug.Log($"🛡 Shield blocked stun on {target.playerName}");
                target.ownedItems.Remove("Shield");
                ProceedToNextPlayer();
                return;
            }

            // Apply stun
            playerStates.SetStunned(target);
            LearningManager.Instance.RecordItemHit(target, "Stun Gun");

            Debug.Log($"🤖 Bot stunned {target.playerName}!");

            // Skip if target was currently playing
            if (playerProfiles[currentPlayerIndex] == target)
            {
                ProceedToNextPlayer();
                return;
            }
            eventScript.currentState = State.Normal;

            ProceedToNextPlayer();
        }

            
    }





}
