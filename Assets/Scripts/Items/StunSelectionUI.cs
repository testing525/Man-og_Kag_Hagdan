using UnityEngine;
using UnityEngine.UI;

public class StunSelectionUI : MonoBehaviour
{
    public GameManager gameManager;
    public GameManager_Bots gameManagerBots;
    public PlayerProfile currentUser;
    public Button[] characterButtons;

    public void Show(PlayerProfile user)
    {
        currentUser = user;

        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            Button btn = characterButtons[i];

            PlayerProfile target = null;

            // ✅ Use whichever manager is assigned
            if (gameManager != null)
                target = gameManager.playerProfiles[index];
            else if (gameManagerBots != null)
                target = gameManagerBots.playerProfiles[index];

            if (target == null)
            {
                btn.interactable = false;
                continue;
            }

            // Only change interactable state
            btn.interactable = target != user;

            // ❗ DO NOT MODIFY COLOR ANYMORE

            // Clear listeners
            btn.onClick.RemoveAllListeners();

            // Assign new stun logic
            btn.onClick.AddListener(() =>
            {
                OnTargetSelected(target);
            });
        }
    }

    private void OnTargetSelected(PlayerProfile target)
    {
        Debug.Log("🧨 Attempting to stun: " + target.playerName);

        // 🔰 Check if target is shielded
        if (target.ownedItems.Contains("Shield"))
        {
            Debug.Log($"🛡 {target.playerName}'s shield blocked the stun!");
            target.ownedItems.Remove("Shield"); // consume shield
            CleanupButtons();
            return;  // Stun canceled
        }

        // 🥊 Apply stun normally
        if (gameManager != null)
            gameManager.playerStates.SetStunned(target);
        else if (gameManagerBots != null) 
        {
            gameManagerBots.playerStates.SetStunned(target);
            gameManagerBots.ProceedToNextPlayer();
        }

        LearningManager.Instance.RecordItemHit(target, "Stun Gun");

        // ⏭ Skip immediately if it’s the current player's turn
        bool isTargetCurrentPlayer = false;
        if (gameManager != null)
            isTargetCurrentPlayer = gameManager.playerProfiles[gameManager.currentPlayerIndex] == target;
        else if (gameManagerBots != null)
            isTargetCurrentPlayer = gameManagerBots.playerProfiles[gameManagerBots.currentPlayerIndex] == target;

        if (isTargetCurrentPlayer)
        {
            Debug.Log("⏭ Immediately skipping stunned player's current turn...");
            if (gameManager != null) 
            {
                EventScript.instance.currentState = State.Normal;
                gameManager.ProceedToNextPlayer();
            }  
            else if (gameManagerBots != null) 
            {
                EventScript.instance.currentState = State.Normal;
                gameManagerBots.ProceedToNextPlayer();

            }
                
        }

        // Reset state
        if (gameManager != null)
            gameManager.eventScript.currentState = State.Normal;
        else if (gameManagerBots != null)
            gameManagerBots.eventScript.currentState = State.Normal;

        CleanupButtons();
    }

    private void CleanupButtons()
    {
        foreach (Button btn in characterButtons)
        {
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
        }
    }
}
