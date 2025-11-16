using System;
using System.Collections.Generic;

[Serializable]
public class PlayerBehaviorData 
{
    // How many times each item was used globally
    public Dictionary<string, int> itemUseFrequency = new Dictionary<string, int>();

    // What item was used on what tile
    public Dictionary<int, Dictionary<string, int>> tileItemUsage = new Dictionary<int, Dictionary<string, int>>();

    // Which items are commonly bought per round
    public Dictionary<int, Dictionary<string, int>> roundItemPurchases = new Dictionary<int, Dictionary<string, int>>();
}
