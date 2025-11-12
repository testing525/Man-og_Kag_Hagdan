using UnityEngine;

public class EventScript : MonoBehaviour
{
    public ShopManager shopManager;
    public int roundCount = 0;
    public State currentState = State.Normal;

    public void OnNewRound()
    {
        roundCount++;
        Debug.Log($"🌀 New Round Started! (Round {roundCount})");

        if (roundCount % 2 == 0)
        {
            shopManager.StartShopPhase();
        }
    }
}

public enum State
{
    Normal,
    Shopping,
    PlayerCheckingInventory,
}
