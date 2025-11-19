using UnityEngine;
using System.Collections;

public class EventScript : MonoBehaviour
{
    public static EventScript instance;
    public ShopManager shopManager;
    public int roundCount = 0;
    public int currentRound;
    public State currentState = State.Normal;
    public GameManager gameManager;
    public GameManager_Bots gameManagerBots;

    private void Awake()
    {
        instance = this;
    }

    public void OnNewRound()
    {
        roundCount++;
        currentRound++;
        Debug.Log($"🌀 New Round Started! (Round {roundCount})");

        StartCoroutine(WaitForPlayerToFinishAndStartShop());
    }

    private IEnumerator WaitForPlayerToFinishAndStartShop()
    {
        // Wait while a player is using an item or checking inventory
        while (currentState == State.PlayerIsUsingItem || currentState == State.PlayerCheckingInventory)
        {
            yield return null; // wait for next frame
        }

        // Now safe to start shop
        if (shopManager != null)
        {
            PlayerProfile[] profiles = null;

            if (gameManager != null)
                profiles = gameManager.playerProfiles;
            else if (gameManagerBots != null)
                profiles = gameManagerBots.playerProfiles;

            if (profiles != null && profiles.Length > 0)
            {
                shopManager.InitializeShop(profiles);
                shopManager.StartShopPhase();
                currentState = State.Shopping;
                Debug.Log("🛍️ Shop phase started.");
            }
        }
    }
}

public enum State
{
    Normal,
    Shopping,
    PlayerCheckingInventory,
    PlayerIsUsingItem,
    PlayerIsPlacingItem
}
