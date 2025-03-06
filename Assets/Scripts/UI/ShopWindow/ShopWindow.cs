using System.Collections.Generic;
using UnityEngine;
using YG;

public class ShopWindow : BaseWindowWithData<ShopWindowData>
{
    [SerializeField] private List<ShopItemTemplate> items = new List<ShopItemTemplate>();
    //PrecreatedWindow, but load images async
    override public async void Show()
    {

    }

    override public async void Hide()
    {

    }
}
