
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class GlobalData : MonoBehaviour
{
    // [SerializeField]    
    public static GlobalData Instance;
    // Start is called before the first frame update
    void Start()
    {
        GlobalData.Instance = this;
        DontDestroyOnLoad(this.gameObject);
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
