using System;
using UnityEngine;
using YG;

public class ShopItemTemplate : MonoBehaviour
{
    [SerializeField] GameObject BuyButton;
    [SerializeField] GameObject SelectButton;
    [SerializeField] GameObject InUseState;
    [SerializeField] GameObject ProductParent;
    [SerializeField] Sprite iconSprite;

    public void Init(Action buyCallback,Action UseCallback){

    }

    public void Buy()
    {

    }
    public void Use()
    {

    }
}
