using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BotStateWriter : MonoBehaviour
{
    private string aiFolder;
    private string statePath;
    private int currentRound;

    private void Awake()
    {
    }

    private void Start()
    {
        currentRound = EventScript.instance.currentRound;

        // Use the same folder as LearningManager
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        aiFolder = Path.Combine(projectRoot, "BotAI");
        if (!Directory.Exists(aiFolder))
        {
            Directory.CreateDirectory(aiFolder);
        }
        else
        {
            Debug.Log("Found ai folder to write bot state");
        }

        statePath = Path.Combine(aiFolder, "state.json");

    }

    public void WriteBotState(PlayerProfile bot, int currentRound)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        aiFolder = Path.Combine(projectRoot, "BotAI");
        if (!Directory.Exists(aiFolder))
            Directory.CreateDirectory(aiFolder);

        statePath = Path.Combine(aiFolder, "state.json");

        BotState state = new BotState
        {
            player = bot.playerName,
            tiles = bot.currentTile,
            points = bot.points,
            ownedItems = new List<string>(bot.ownedItems),
            round = currentRound
        };

        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(statePath, json);

        Debug.Log($"✅ Saved state for {bot.playerName} to {statePath}");
    }

    // Path for Python to write result
    public string ResultPath
    {
        get
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string aiFolder = Path.Combine(projectRoot, "BotAI");
            if (!Directory.Exists(aiFolder))
                Directory.CreateDirectory(aiFolder);

            return Path.Combine(aiFolder, "result.json");
        }
    }


    [System.Serializable]
    private class BotStateListWrapper
    {
        public List<BotState> bots;
    }
}

[System.Serializable]
public class BotState
{
    public string player;            // PlayerProfile.name
    public int tiles;                // Current tile
    public int points;               // Current points
    public List<string> ownedItems;  // Current items
    public int round;                // Current round
}


