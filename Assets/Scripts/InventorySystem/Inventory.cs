using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using InventoryNamespace;
using UnityEngine;
using YG;

public class Inventory
{
    private List<uint> collectedItems;
    private List<uint> equipedItems;
    private Item equipedCircleSkin;
    private Item equipedStickSkin;
    private bool isInit = false;
    public async Task Init(){
        FromSave();
        if(!isInit){
        await InitDefault();
        }
    }

    private async Task InitDefault(){
        isInit = true;
        equipedCircleSkin = await ItemFabric.GetItem(0);
        equipedStickSkin = await ItemFabric.GetItem(1);
    }

    public void Add(Item item)
    {
        collectedItems.Add(item.ID);
    }

    private void Save()
    {
        YG2.saves.collectedItems = collectedItems;
        YG2.saves.equipedItems = equipedItems;
        YG2.saves.equipedCircleSkin = equipedCircleSkin;
        YG2.saves.equipedStickSkin = equipedStickSkin;
        YG2.SaveProgress();
    }
    private void FromSave()
    {
        collectedItems = YG2.saves.collectedItems;
        equipedItems = YG2.saves.equipedItems;
        equipedCircleSkin = YG2.saves.equipedCircleSkin;
        equipedStickSkin = YG2.saves.equipedStickSkin;
    }

    public async UniTask<Item> EquipedCircleSkin(){
        await equipedCircleSkin.Init();
        return equipedCircleSkin;
    }
    public async UniTask<Item> EquipedStickSkin(){
        await equipedStickSkin.Init();
        return equipedStickSkin;
    }

}
