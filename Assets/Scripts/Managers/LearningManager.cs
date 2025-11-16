using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class LearningManager : MonoBehaviour
{
    public static LearningManager Instance;

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
            savePath = Path.Combine(Application.persistentDataPath, "botLearning.json");
            Load();
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
        if (!data.itemUseFrequency.ContainsKey(item))
            data.itemUseFrequency[item] = 0;

        data.itemUseFrequency[item]++;

        // Tile-based usage
        if (!data.tileItemUsage.ContainsKey(tile))
            data.tileItemUsage[tile] = new Dictionary<string, int>();

        if (!data.tileItemUsage[tile].ContainsKey(item))
            data.tileItemUsage[tile][item] = 0;

        data.tileItemUsage[tile][item]++;

        Save();
    }

    public void RecordItemBought(string item, int round)
    {
        if (!data.roundItemPurchases.ContainsKey(round))
            data.roundItemPurchases[round] = new Dictionary<string, int>();

        if (!data.roundItemPurchases[round].ContainsKey(item))
            data.roundItemPurchases[round][item] = 0;

        data.roundItemPurchases[round][item]++;

        Save();
    }
}
