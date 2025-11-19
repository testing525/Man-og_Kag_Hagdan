using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LearningManager : MonoBehaviour
{
    public static LearningManager Instance;

    [Header("References")]
    public GameManager gameManager;
    public GameManager_Bots gameManagerBots;

    //pyTorch connections
    private string aiFolder;
    private string statePath;
    private string resultPath;


    private string savePath;
    public PlayerBehaviorData data = new PlayerBehaviorData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            savePath = Path.Combine(projectRoot, "botLearning.json");
            Load();

            // PyTorch communication folder

            // Example: "C:/Users/.../Slime/Project/"
            aiFolder = Path.Combine(projectRoot, "BotAI");

            if (!Directory.Exists(aiFolder))
                Directory.CreateDirectory(aiFolder);

            statePath = Path.Combine(aiFolder, "state.json");
            resultPath = Path.Combine(aiFolder, "result.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // SAVE & LOAD -----------------------------------------
    public void Save()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("📁 Learning data saved.");
    }
     
    public void Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<PlayerBehaviorData>(json);
            Debug.Log("📂 Learning data loaded.");
        }
        else
        {
            data = new PlayerBehaviorData();
            Debug.Log("📄 No learning file, starting fresh.");
        }
    }

    // UPDATE FUNCTIONS --------------------------------------
    public void RecordItemUsed(string item, int tile)
    {
        // Global usage
        var global = data.itemUseFrequencyList.Find(x => x.key == item);
        if (global != null)
            global.value++;
        else
            data.itemUseFrequencyList.Add(new KeyValueStringInt { key = item, value = 1 });

        // Tile-based usage
        var tileData = data.tileItemUsageList.Find(x => x.tile == tile);
        if (tileData == null)
        {
            tileData = new TileItemUsage { tile = tile };
            data.tileItemUsageList.Add(tileData);
        }

        var tileItem = tileData.items.Find(x => x.key == item);
        if (tileItem != null)
            tileItem.value++;
        else
            tileData.items.Add(new KeyValueStringInt { key = item, value = 1 });

        Save();
    }

    public void RecordItemBought(string item, int round)
    {
        var roundData = data.roundItemPurchasesList.Find(x => x.round == round);
        if (roundData == null)
        {
            roundData = new RoundItemPurchases { round = round };
            data.roundItemPurchasesList.Add(roundData);
        }

        var roundItem = roundData.items.Find(x => x.key == item);
        if (roundItem != null)
            roundItem.value++;
        else
            roundData.items.Add(new KeyValueStringInt { key = item, value = 1 });

        Save();
    }

    public void RecordItemHit(PlayerProfile player, string itemName)
    {
        var hitEvent = new ItemHitEvent
        {
            itemName = itemName,
            tile = player.currentTile, // current tile when hit
            playerPlace = GetPlayerPlace(player) // rank / placement
        };

        data.itemHitEvents.Add(hitEvent);
        Save();

        Debug.Log($"💥 Recorded {itemName} hit on {player.playerName} at tile {hitEvent.tile} (place {hitEvent.playerPlace})");
    }

    public int GetPlayerPlace(PlayerProfile player)
    {
        PlayerProfile[] profiles = null;

        // Select correct gameManager
        if (gameManager != null)
        {
            profiles = gameManager.playerProfiles;
        }
        else if (gameManagerBots != null)
        {
            profiles = gameManagerBots.playerProfiles;
        }
        else
        {
            Debug.LogWarning("No game manager found!");
            return -1;
        }

        // Sort by tile descending
        var sorted = profiles.OrderByDescending(p => p.currentTile).ToArray();

        // Find player's position
        for (int i = 0; i < sorted.Length; i++)
        {
            if (sorted[i] == player)
                return i + 1;   // 1 = 1st place
        }

        return sorted.Length; // fallback (last place)
    }



    public void WriteStateForPython(object gameState)
    {
        string json = JsonUtility.ToJson(gameState, true);
        File.WriteAllText(statePath, json);
    }

    public T ReadPythonResult<T>()
    {
        if (!File.Exists(resultPath))
            return default;

        string json = File.ReadAllText(resultPath);
        return JsonUtility.FromJson<T>(json);
    }

    public void ExportTrainingData()
    {
        string path = Path.Combine(aiFolder, "training_data.json");
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log("📤 Exported training data for Python.");
    }



}

[System.Serializable]
public class GameState
{
    public int playerTile;
    public int round;
}

