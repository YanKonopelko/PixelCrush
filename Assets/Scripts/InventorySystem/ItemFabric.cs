using UnityEngine;
using InventoryNamespace;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ItemFabric
{
    private static Dictionary<uint, Item> loadedItems = new Dictionary<uint, Item>();
    public static async UniTask<Item> GetItem(uint itemId)
    {
        // if (!loadedItems.ContainsKey(itemId))
        // {
            int itemIdx = GlobalData.Instance.AllItems.FindIndex(a => a.ID == itemId);
            if (itemIdx == -1)
            {
                Debug.LogWarning($"Cannot find item with{itemId}");
                return null;
            }
            else
            {
                await GlobalData.Instance.AllItems[itemIdx].Init();
                return GlobalData.Instance.AllItems[itemIdx];
                // item.Init();
                // loadedItems.Add(itemId,item);
            }
        // }
        Item retItem = new Item();
        loadedItems.TryGetValue(itemId,out retItem);
        return retItem;
    }
}
