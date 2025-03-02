
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using InventoryNamespace;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class GlobalData : MonoBehaviour
{
    [SerializeField] public Pool pool;
    [SerializeField] public UIManager UIManager;
    [SerializeField] public SoundManager SoundManager;
    [SerializeField] public MusicManager MusicManager;
    public static GlobalData Instance;
    [SerializeField] private GameObject particlePrefab;

    [SerializeField] private GameObject[] particlePrefabs;

    [SerializeField] private CustomArrayWithEnum<EBrusherStickMaterialType, Material>[] stickMaterials;
    [SerializeField] private CustomArrayWithEnum<EBrusherStickSkinType, Mesh>[] stickMeshes;

    [SerializeField] private CustomArrayWithEnum<EBrusherCircleMaterialType, Material>[] circleMaterials;
    [SerializeField] private CustomArrayWithEnum<EBrusherCircleSkinType, Material>[] circleMeshes;
    [SerializeField] public List<LevelConfig> levelConfigss;
    [SerializeField] private List<Item> allItems;
    [SerializeField] TextAsset itemsJSON;


    [DllImport("__Internal")]
    private static extern bool IsReleaseVersion();
    [DllImport("__Internal")]
    private static extern int GetBuildNumber();

    private bool isRelease;
    public bool IsRelease { get { return isRelease; } }
    public List<Item> AllItems { get { return allItems; } }
    public Inventory Inventory{get;private set;}

    async Task Awake()
    {
        GlobalData.Instance = this;
        DontDestroyOnLoad(this.gameObject);
#if !UNITY_EDITOR && UNITY_WEBGL

    try
    {
        isRelease = IsReleaseVersion();
    }
    catch(Exception e){
        Debug.Log(e);
    }
#else
        isRelease = false;
#endif
        MusicManager.Init();
        SoundManager.Init();
        Inventory = new Inventory();
    }

    public async UniTask Init()
    {
        pool.Init();
        pool.PreparePool(particlePrefab, 50);
        string str = itemsJSON.text;
        ItemList itemList = JsonConvert.DeserializeObject<ItemList>(str);
        allItems = itemList.Items;
        for(int i = 0; i < allItems.Count;i++){
            allItems[i].Init();
        }
        await Inventory.Init();
    }

    public async UniTask<Texture2D> GetLevelTexture(int LevelNum)
    {
        Debug.Log("Load level:" + LevelNum.ToString());
        UniTask<Texture2D> textureHandle = Addressables.LoadAssetAsync<Texture2D>($"Level_{LevelNum}").Task.AsUniTask();
        // Addressables.load
        Texture2D texture2D = await textureHandle;
        return texture2D;
    }
    public void UnloadLevelTexture(int LevelNum)
    {
        Debug.Log("UnLoad level:" + LevelNum.ToString());
        var textureHandle = Addressables.LoadAssetAsync<Texture2D>($"Level_{LevelNum}");
        textureHandle.Release();
    }
}
