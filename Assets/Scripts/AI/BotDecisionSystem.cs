using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class BotDecisionSystem : MonoBehaviour
{
    public static BotDecisionSystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    // -----------------------
    // BUY ITEM DECISION
    // -----------------------
    public IEnumerator DecideItemToBuyRoutine(PlayerProfile bot, System.Action<ItemData> callback)
    {
        // small delay for realism
        yield return new WaitForSeconds(0.5f);

        // 1️⃣ Write bot state for Python
        BotStateWriter writer = FindAnyObjectByType<BotStateWriter>();
        if (writer != null)
        {
            writer.WriteBotState(bot, EventScript.instance.currentRound);
        }
        else
        {
            Debug.LogWarning("⚠️ BotStateWriter not found in scene!");
            callback?.Invoke(null);
            yield break;
        }

        // Delete previous result if exists
        if (File.Exists(writer.ResultPath))
            File.Delete(writer.ResultPath);

        // 2️⃣ Run Python asynchronously
        _ = PythonRunner.RunPythonAsync("brain.py");

        // 3️⃣ Wait for Python to create result.json
        float timeout = 8f;
        float timer = 0f;
        string recommendedItem = null;

        while (!File.Exists(writer.ResultPath) && timer < timeout)
        {
            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // 4️⃣ Read Python result
        if (File.Exists(writer.ResultPath))
        {
            try
            {
                string json = File.ReadAllText(writer.ResultPath);
                var result = JsonUtility.FromJson<BotPythonResult>(json);
                recommendedItem = result.itemToBuy;
                Debug.Log($"🤖 Python recommended item for {bot.playerName}: {recommendedItem}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Failed to read Python result: {ex.Message}");
            }
        }

        // Fallback: use hardcoded logic if Python failed
        if (string.IsNullOrEmpty(recommendedItem))
        {
            recommendedItem = DecideItemToBuy(bot, ShopManager.Instance.displayedItems);
        }

        // 6️⃣ Return the selected ItemData
        ItemData itemData = string.IsNullOrEmpty(recommendedItem) ? null : ShopManager.Instance.GetItemData(recommendedItem);
        callback?.Invoke(itemData);
    }


    public string DecideItemToBuy(PlayerProfile bot, List<ItemData> availableItems)
    {
        // 1. ff bot has > 3 items, skip buying
        if (bot.ownedItems.Count >= 3)
            return null;

        // 2. prioritize survival if near dangers
        if (IsNearSnake(bot) && ContainsItem(availableItems, "Anti Snake Spray"))
            return "Anti Snake Spray";

        // 3. bot prefers Pogo Stick (movement advantage)
        if (ContainsItem(availableItems, "Pogo Stick"))
            return "Pogo Stick";

        // 4. bot buys Bomb if someone is ahead
        if (IsPlayerAhead(bot) && ContainsItem(availableItems, "Bomb"))
            return "Bomb";

        // 5. buy Shield if no shield owned
        if (!bot.ownedItems.Contains("Shield") && ContainsItem(availableItems, "Shield"))
            return "Shield";

        // 6. No intelligent option → skip
        return null;
    }

    private bool ContainsItem(List<ItemData> list, string name)
    {
        return list.Exists(i => i.itemName == name);
    }


    // -----------------------
    // USE ITEM DECISION
    // -----------------------
    public IEnumerator DecideUseItem(PlayerProfile bot, System.Action<string> callback)
    {
        // small delay for realism
        yield return new WaitForSeconds(0.5f);

        // write bot state for Python
        BotStateWriter writer = FindAnyObjectByType<BotStateWriter>();
        if (writer != null)
        {
            writer.WriteBotState(bot, EventScript.instance.currentRound);
        }
        else
        {
            Debug.LogWarning("⚠️ BotStateWriter not found in scene!");
            callback?.Invoke(null);
            yield break;
        }

        // delete previous result if exists
        if (File.Exists(writer.ResultPath))
            File.Delete(writer.ResultPath);

        // run Python asynchronously
        _ = PythonRunner.RunPythonAsync("brain_itemUse.py");

        // 3️⃣ Wait for Python to create result.json
        float timeout = 8f;
        float timer = 0f;
        string recommendedItem = null;

        while (!File.Exists(writer.ResultPath) && timer < timeout)
        {
            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // read Python result
        if (File.Exists(writer.ResultPath))
        {
            try
            {
                string json = File.ReadAllText(writer.ResultPath);
                var result = JsonUtility.FromJson<BotPythonResult>(json);
                recommendedItem = result.itemToBuy;

                Debug.Log($"🤖 Python recommended item for {bot.playerName}: {recommendedItem}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Failed to read Python result: {ex.Message}");
            }
        }

        // use hardcoded logic if Python failed
        if (string.IsNullOrEmpty(recommendedItem))
        {
            // Danger avoidance
            if (IsNearSnake(bot) && bot.ownedItems.Contains("Anti Snake Spray"))
                recommendedItem = "Anti Snake Spray";
            // Shield
            else if (bot.ownedItems.Contains("Shield") && IsLikelyToGetAttacked(bot))
                recommendedItem = "Shield";
            // Bomb
            else if (bot.ownedItems.Contains("Bomb") && IsPlayerAhead(bot))
                recommendedItem = "Bomb";
            // Pogo Stick
            else if (bot.ownedItems.Contains("Pogo Stick"))
                recommendedItem = "Pogo Stick";
        }

        callback?.Invoke(recommendedItem);
    }

    //SPECIAL ITEMS USE:
    public PlayerProfile ChooseStunTarget(PlayerProfile bot)
    {
        PlayerProfile best = null;

        foreach (var p in GameManager_Bots.instance.playerProfiles)
        {
            if (p == bot) continue;            // cant stun self
            if (GameManager_Bots.instance.playerStates.GetState(p) == PlayerState.Stunned) continue; // skip already stunned
            if (p.ownedItems.Contains("Shield")) continue; // skip shielded players

            // stun player closest to winning
            if (best == null || p.currentTile > best.currentTile)
                best = p;
        }

        return best;
    }




    // -----------------------
    // HELPER LOGIC
    // -----------------------
    private bool IsPlayerAhead(PlayerProfile bot)
    {
        return true; // placeholder
    }

    private bool IsNearSnake(PlayerProfile bot)
    {
        return false; // placeholder
    }

    private bool IsLikelyToGetAttacked(PlayerProfile bot)
    {
        return Random.value < 0.3f;
    }
}



