
using System.Collections.Generic;
using System.IO;
using InventoryNamespace;
using UnityEngine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using UnityEditor;
// [ExecuteInEditMode]
public class ItemSerializier : MonoBehaviour
{
    [SerializeField] List<Item> items;
    [SerializeField] TextAsset directInputFile;
    [SerializeField] string outputFilePath = "Assets/Resources/Items/Items.json";
    [SerializeField] string inputFilePath = "Assets/Resources/Items/Items.json";
    [SerializeField] string outputField;
    public void FromJson()
    {
        if (inputFilePath == "" && directInputFile == null) return;
        string str = "";
        if (directInputFile)
        {
            str = directInputFile.text;
        }
        else
        {
            if(!File.Exists(inputFilePath)){
                Debug.LogWarning("Incorect input path");
                return;
            }
            StreamReader rider = new StreamReader(inputFilePath, false);
            str = rider.ReadToEnd();
        }
        ItemList itemList = JsonConvert.DeserializeObject<ItemList>(str);
        items = new List<Item>();
        for(int i = 0; i < itemList.Items.Count;i++){
            Item _item = itemList.Items[i];
            _item.Init();
            // Item fullItem = new Item();
            // fullItem.Init();
            items.Add(_item);
        }
    }
    public string AsJson()
    {
        // if(outputFilePath)
        ItemList list = new ItemList();
        list.Items = items;
        string jsonString = JsonConvert.SerializeObject(list);
        outputField = jsonString;
        string path = outputFilePath;
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();
        AssetDatabase.ImportAsset(path);
        Debug.Log(jsonString);
        return jsonString;
    }
}
