using UnityEngine;
using InventoryNamespace;
using UnityEditor.PackageManager;
using System.Collections.Generic;

public class ItemFabric
{
    private static Dictionary<uint, Item> loadedItems = new Dictionary<uint, Item>();
    public static Item GetItem(uint itemId)
    {
        if (!loadedItems.ContainsKey(itemId))
        {
            int itemIdx = GlobalData.Instance.AllItems.FindIndex(a => a.ID == itemId);
            if (itemIdx == -1)
            {
                Debug.LogWarning($"Cannot find item with{itemId}");
                return null;
            }
            else
            {
                Item item = GlobalData.Instance.AllItems[itemIdx];
                item.Init();
                loadedItems.Add(itemId,item);
            }
        }
        Item retItem = new Item();
        loadedItems.TryGetValue(itemId,out retItem);
        return retItem;
    }
}
