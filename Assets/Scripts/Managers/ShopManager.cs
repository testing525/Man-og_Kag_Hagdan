using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("References")]
    public Transform itemLayout;           // Parent that holds all 6 ItemContainers
    public Image[] itemImages;             // Auto-filled or manually assigned (6)
    public TMP_Text nameText;
    public TMP_Text costText;
    public Button buyButton;

    [Header("UI")]
    public TMP_Text timerText;             // Timer UI (assign in Inspector)
    public TMP_Text playerTurnText;        // Player turn UI (assign in Inspector)

    [Header("Settings")]
    public float shopTimePerPlayer = 10f;  // Duration each player can shop
    public int totalPlayers = 4;           // Change dynamically later

    [Header("Item Data")]
    public List<ItemData> allItems;        // All possible items in the game

    private ItemData selectedItem;
    private int currentPlayer = 0;
    private Coroutine shopRoutine;
    private EventScript eventScript;       // to set state back to Normal later

    void Start()
    {
        // Find event script in scene (optional)
        eventScript = FindAnyObjectByType<EventScript>();

        // Ensure the shop starts hidden
        gameObject.SetActive(false);
    }

    // ────────────────────────────────
    // Called by EventScript
    // ────────────────────────────────
    public void StartShopPhase()
    {
        DisplayRandomItems();
        gameObject.SetActive(true);
        Debug.Log("🛍️ Shop phase started!");

        if (eventScript != null)
            eventScript.currentState = State.Shopping;

        currentPlayer = 1;
        shopRoutine = StartCoroutine(HandlePlayerTurns());
    }

    // ────────────────────────────────
    // Handles each player's shop turn
    // ────────────────────────────────
    private IEnumerator HandlePlayerTurns()
    {
        while (currentPlayer <= totalPlayers)
        {
            playerTurnText.text = $"Player{currentPlayer} Turn";
            yield return StartCoroutine(RunShopTimer());
            currentPlayer++;
        }

        EndShopPhase();
    }

    // ────────────────────────────────
    // Countdown timer for each turn
    // ────────────────────────────────
    private IEnumerator RunShopTimer()
    {
        float timeLeft = shopTimePerPlayer;

        while (timeLeft > 0)
        {
            timerText.text = $"{Mathf.CeilToInt(timeLeft)}s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        timerText.text = " ";
        yield return new WaitForSeconds(0.5f);
    }

    // ────────────────────────────────
    // Closes shop after all players
    // ────────────────────────────────
    public void EndShopPhase()
    {
        if (shopRoutine != null)
        {
            StopCoroutine(shopRoutine);
            shopRoutine = null;
        }

        gameObject.SetActive(false);
        timerText.text = "";
        playerTurnText.text = "";

        if (eventScript != null)
            eventScript.currentState = State.Normal;

        Debug.Log("🚪 Shop phase ended, back to Normal state.");
    }

    // ────────────────────────────────
    // Item display + selection logic
    // ────────────────────────────────
    void DisplayRandomItems()
    {
        List<ItemData> randomized = new List<ItemData>(allItems);
        randomized.Shuffle();
        int itemCount = Mathf.Min(6, randomized.Count);

        if (itemImages == null || itemImages.Length == 0)
        {
            itemImages = new Image[itemLayout.childCount];
            for (int i = 0; i < itemLayout.childCount; i++)
                itemImages[i] = itemLayout.GetChild(i).Find("Item").GetComponent<Image>();
        }

        for (int i = 0; i < itemCount; i++)
        {
            int index = i;
            ItemData item = randomized[i];
            itemImages[i].sprite = item.itemSprite;
            itemImages[i].preserveAspect = true;

            Button itemButton = itemImages[i].GetComponent<Button>();
            if (itemButton == null)
                itemButton = itemImages[i].gameObject.AddComponent<Button>();

            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => SelectItem(item));
        }

        for (int i = itemCount; i < itemImages.Length; i++)
        {
            itemImages[i].sprite = null;
        }
    }

    void SelectItem(ItemData item)
    {
        selectedItem = item;
        nameText.text = item.itemName;
        costText.text = $"Cost: {item.cost} pts";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuySelectedItem);
    }

    void BuySelectedItem()
    {
        if (selectedItem == null) return;
        Debug.Log($"🛒 Player {currentPlayer} bought: {selectedItem.itemName} for {selectedItem.cost} points");
        // TODO: Deduct points, add item to inventory
    }
}

// ────────────────────────────────
// Shuffle helper
// ────────────────────────────────
public static class ListExtensions
{
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}

// ────────────────────────────────
// Item data
// ────────────────────────────────
[System.Serializable]
public class ItemData
{
    public string itemName;
    public Sprite itemSprite;
    public int cost;
}
