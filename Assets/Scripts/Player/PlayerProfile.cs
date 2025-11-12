using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public string playerName;
    public int points;

    public PlayerProfile(string name)
    {
        playerName = name;
        points = 0;
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
