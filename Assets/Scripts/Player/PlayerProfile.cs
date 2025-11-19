using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public string playerName;
    public int currentTile;
    public int points;
    public bool isBot;
    public int crowns = 0;
    public List<string> ownedItems = new List<string>();


    public PlayerProfile(string name)
    {
        playerName = name;
        points = 0;
        ownedItems = new List<string>();

    }

    public bool AddItem(string itemName)
    {
        if (ownedItems.Count >= 3)
        {
            Debug.Log($"{playerName} already has 3 items. Cannot add more.");
            return false;
        }

        ownedItems.Add(itemName);
        return true;
    }





    public void AddPoints(int value)
    {
        points += value;
    }

    public void ResetPoints()
    {
        points = 0;
    }
}
