using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InventoryNamespace
{
    [Serializable]
    public class Item
    {
        [SerializeField]
        public int ID;
        [SerializeField]
        public string Name;
        [SerializeField]
        public string Description;
        [SerializeField]
        public EItemType ItemType;
        [SerializeField]
        [JsonIgnore]
        public Mesh Model;
        [SerializeField]
        [JsonIgnore]
        public Material Material;
        [SerializeField]
        [JsonIgnore]
        public Sprite Icon;
        [SerializeField]
        public string ModelName;
        [SerializeField]
        public string MaterialName;
        [SerializeField]
        public string IconName;

        public async void Init()
        {
            try
            {
                Model = await Addressables.LoadAssetAsync<Mesh>(ModelName);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            try
            {
                Material = await Addressables.LoadAssetAsync<Material>(MaterialName);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            try
            {
                Icon = await Addressables.LoadAssetAsync<Sprite>(IconName);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

    }

    public struct ItemList
    {
        [JsonProperty("Items")]
        public List<Item> Items;
    }
}

