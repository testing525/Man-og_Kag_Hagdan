using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemCostText;
    public TMP_Text itemDescText;
    public Button buyButton;

    private ShopItemData currentItem;

    public void Setup(ShopItemData data)
    {
        currentItem = data;

        itemIcon.sprite = data.icon;
        itemNameText.text = data.itemName;
        itemCostText.text = data.cost.ToString() + " pts";
        itemDescText.text = data.description;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => BuyItem());
    }

    private void BuyItem()
    {
        Debug.Log($"🛒 Bought {currentItem.itemName} for {currentItem.cost} points!");
        // TODO: Deduct player points and add item to inventory
    }
}
