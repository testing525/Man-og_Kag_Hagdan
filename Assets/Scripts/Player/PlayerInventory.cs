using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Slots")]
    public Transform[] itemSlots;        // Assign ItemContainer1, ItemContainer2, ItemContainer3
    public Button cancelButton;          // Assign CancelButton

    [Header("References")]
    public GameManager gameManager;      // Reference to the GameManager
    public GameManager_Bots gameManagerBots; // Optional Bots version
    public ShopManager shopManager;      // To fetch ItemData
    public EventScript eventManager;

    private PlayerProfile currentPlayer;

    void Start()
    {
        // Hide inventory at start
        gameObject.SetActive(false);

        // Set up Cancel button listener
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancel);
        }
    }

    /// <summary>
    /// Show the inventory for the current player
    /// </summary>
    public void ShowInventory(PlayerProfile playerProfile)
    {
        currentPlayer = playerProfile;
        gameObject.SetActive(true);

        if (eventManager != null)
            eventManager.currentState = State.PlayerCheckingInventory;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            Transform slot = itemSlots[i];
            Image itemImg = slot.Find("ItemImg")?.GetComponent<Image>();
            Button btn = slot.Find("Button")?.GetComponent<Button>();

            if (itemImg == null || btn == null) continue;

            btn.onClick.RemoveAllListeners();

            if (i < currentPlayer.ownedItems.Count)
            {
                string itemName = currentPlayer.ownedItems[i];
                ItemData data = shopManager != null ? shopManager.GetItemData(itemName) : null;

                slot.gameObject.SetActive(true);

                if (data != null && data.itemSprite != null)
                {
                    itemImg.sprite = data.itemSprite;
                    itemImg.color = Color.white;
                }
                else
                {
                    itemImg.sprite = null;
                    itemImg.color = Color.clear;
                }

                int slotIndex = i;
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("Item clicked");
                    if (gameManager != null)
                        gameManager.OnItemUsed(currentPlayer, currentPlayer.ownedItems[slotIndex]);
                    else if (gameManagerBots != null)
                        gameManagerBots.OnItemUsed(currentPlayer, currentPlayer.ownedItems[slotIndex]);
                });
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
        if (eventManager != null)
            eventManager.currentState = State.Normal;

        // Automatically proceed to next player if canceled
        if (gameManager != null)
            gameManager.ProceedToNextPlayer();
        else if (gameManagerBots != null)
            gameManagerBots.ProceedToNextPlayer();
    }
}
