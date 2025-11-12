using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    [Header("General Info")]
    public ItemType itemType;
    public string itemName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public int cost;

    [Header("Effect Settings (optional)")]
    public int duration; // For timed effects (like Shield)
    public int magnitude; // Power of the item (like +4 for Pogo Stick)

    // You can extend this later if needed (e.g., range, cooldown, etc.)
}
