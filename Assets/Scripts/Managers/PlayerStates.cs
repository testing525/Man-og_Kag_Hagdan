using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Normal,
    Stunned,
    Shielded,
    PointsMultiplier
}

public class PlayerStates : MonoBehaviour
{
    // How many rounds a stun lasts
    public int stunDuration = 3;

    // Stores each player's state + remaining turns
    private Dictionary<PlayerProfile, PlayerStateData> stateData = new Dictionary<PlayerProfile, PlayerStateData>();


    public void RegisterPlayer(PlayerProfile profile)
    {
        if (!stateData.ContainsKey(profile))
            stateData.Add(profile, new PlayerStateData());
    }

    public PlayerState GetState(PlayerProfile profile)
    {
        return stateData[profile].state;
    }

    public void SetStunned(PlayerProfile profile)
    {
        stateData[profile].state = PlayerState.Stunned;
        stateData[profile].remainingTurns = stunDuration;
        Debug.Log($"⚡ {profile.playerName} has been stunned for {stunDuration} turns!");
    }

    public void SetShielded(PlayerProfile profile)
    {
        stateData[profile].state = PlayerState.Shielded;
        stateData[profile].remainingTurns = stunDuration;
        Debug.Log($"⚡ {profile.playerName} has been shielded for {stunDuration} turns!");
    }

    public void SetPointsMultiplier(PlayerProfile profile)
    {
        stateData[profile].state = PlayerState.PointsMultiplier;
        stateData[profile].remainingTurns = stunDuration;

        Debug.Log($"⭐ {profile.playerName} has activated Points Multiplier for {stunDuration} turns!");
    }

    public void RemoveState(PlayerProfile profile)
    {
        stateData[profile].state = PlayerState.Normal;
        stateData[profile].remainingTurns = 0;

        Debug.Log($"🔄 {profile.playerName}'s state reset to Normal.");
    }


    // Called every time a player's turn STARTS
    public bool ProcessStateAtTurnStart(PlayerProfile profile)
    {
        var data = stateData[profile];

        // Decrement remainingTurns for any timed state
        if (data.state == PlayerState.Stunned || data.state == PlayerState.PointsMultiplier || data.state == PlayerState.Shielded)
        {
            data.remainingTurns--;

            Debug.Log($"⏳ {profile.playerName} is affected ({data.state}). Turns left: {data.remainingTurns}");

            if (data.remainingTurns <= 0)
            {
                Debug.Log($"✅ {profile.playerName} returned to normal state!");
                data.state = PlayerState.Normal;
            }
        }

        // Only skip turn if stunned
        return data.state == PlayerState.Stunned;
    }


    private class PlayerStateData
    {
        public PlayerState state = PlayerState.Normal;
        public int remainingTurns = 0;
    }
}
