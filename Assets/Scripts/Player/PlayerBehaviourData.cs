using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerBehaviorData
{
    public List<KeyValueStringInt> itemUseFrequencyList = new List<KeyValueStringInt>();
    public List<TileItemUsage> tileItemUsageList = new List<TileItemUsage>();
    public List<RoundItemPurchases> roundItemPurchasesList = new List<RoundItemPurchases>();
    public List<ItemHitEvent> itemHitEvents = new List<ItemHitEvent>();
}


[System.Serializable]
public class ItemHitEvent
{
    public string itemName;
    public int tile;
    public int playerPlace; // 1 = 1st, 2 = 2nd, etc.
}

[System.Serializable]
public class KeyValueStringInt
{
    public string key;
    public int value;
}

[System.Serializable]
public class TileItemUsage
{
    public int tile;
    public List<KeyValueStringInt> items = new List<KeyValueStringInt>();
}

[System.Serializable]
public class RoundItemPurchases
{
    public int round;
    public List<KeyValueStringInt> items = new List<KeyValueStringInt>();
}

