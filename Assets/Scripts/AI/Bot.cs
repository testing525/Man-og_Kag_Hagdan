using System.Collections.Generic;
using UnityEngine;

public class Bot
{
    public PlayerProfile profile;
    public int playerIndex;        // index in playerProfiles
    public int currentTile;
    public GameManager_Bots gameManager;
    public ShopManager shopManager; // reference to the shop


    public Bot(PlayerProfile profile, int playerIndex, GameManager_Bots gm)
    {
        this.profile = profile;
        this.playerIndex = playerIndex;
        this.gameManager = gm;
        this.currentTile = 1; // start tile
    }

    // Get the nearest ladder and snake tiles ahead
    public int GetNextLadderTile()
    {
        foreach (int ladder in gameManager.ladderTiles)
        {
            if (ladder > currentTile)
                return ladder;
        }
        return -1; // no ladder ahead
    }

    public int GetNextSnakeTile()
    {
        foreach (int snake in gameManager.snakeTiles)
        {
            if (snake > currentTile)
                return snake;
        }
        return -1; // no snake ahead
    }

    // Bot decides to buy an item during its shop phase
    public void SmartBuyItem()
    {
        if (shopManager == null || shopManager.allItems.Count == 0)
        {
            Debug.Log($"⚠️ {profile.playerName} cannot buy items, shop empty or missing reference.");
            return;
        }

        // Make sure it's this bot's turn
        if (gameManager.playerProfiles[gameManager.currentPlayerIndex] != profile)
            return;

        var learning = LearningManager.Instance;
        if (learning == null)
        {
            Debug.LogWarning("⚠️ LearningManager not found. Bot will pick randomly.");
            PickRandomItem();
            return;
        }

        // Get candidate items available in the shop
        List<ItemData> candidates = new List<ItemData>(shopManager.allItems);

        // Remove items bot cannot afford
        candidates.RemoveAll(item => item.cost > profile.points);
        if (candidates.Count == 0)
        {
            Debug.Log($"❌ {profile.playerName} cannot afford any items this turn.");
            return;
        }

        // Rank candidates based on LearningManager data
        Dictionary<ItemData, int> itemScores = new Dictionary<ItemData, int>();
        foreach (var item in candidates)
        {
            int score = 0;

            // Tile-based usage priority
            if (learning.data.tileItemUsage.ContainsKey(profile.currentTile) &&
                learning.data.tileItemUsage[profile.currentTile].ContainsKey(item.itemName))
            {
                score += learning.data.tileItemUsage[profile.currentTile][item.itemName];
            }

            // Round-based purchase priority
            int currentRound = gameManager.eventScript.roundCount;
            if (learning.data.roundItemPurchases.ContainsKey(currentRound) &&
                learning.data.roundItemPurchases[currentRound].ContainsKey(item.itemName))
            {
                score += learning.data.roundItemPurchases[currentRound][item.itemName];
            }

            itemScores[item] = score;
        }

        // Pick the item with the highest score
        ItemData chosenItem = null;
        int maxScore = -1;
        foreach (var kvp in itemScores)
        {
            if (kvp.Value > maxScore)
            {
                maxScore = kvp.Value;
                chosenItem = kvp.Key;
            }
        }

        // If no item has a score (first rounds), pick randomly
        if (chosenItem == null)
            chosenItem = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        // Buy the item
        bool added = profile.AddItem(chosenItem.itemName);
        if (added)
        {
            profile.points -= chosenItem.cost;
            shopManager.scoreManager?.UpdateScoreUI();
            Debug.Log($"🤖 {profile.playerName} bought (learning-based): {chosenItem.itemName}");

            // Record purchase in learning data
            learning.RecordItemBought(chosenItem.itemName, gameManager.eventScript.roundCount);
        }
        else
        {
            Debug.Log($"❌ {profile.playerName}'s inventory full, cannot buy {chosenItem.itemName}");
        }
    }

    // Fallback random pick if LearningManager is missing
    private void PickRandomItem()
    {
        List<ItemData> affordable = shopManager.allItems.FindAll(item => item.cost <= profile.points);
        if (affordable.Count == 0) return;

        ItemData chosenItem = affordable[UnityEngine.Random.Range(0, affordable.Count)];
        bool added = profile.AddItem(chosenItem.itemName);
        if (added)
        {
            profile.points -= chosenItem.cost;
            shopManager.scoreManager?.UpdateScoreUI();
            Debug.Log($"🤖 {profile.playerName} bought (random fallback): {chosenItem.itemName}");
        }
    }


}
