using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct CustomArrayWithEnum<Key, Value>
{
    [SerializeField] public Key key;
    [SerializeField] public Value value;
}


public class UIManager : MonoBehaviour
{
    [SerializeField] List<CustomArrayWithEnum<EWindowType, GameObject>> windows;
    [SerializeField]  GameObject layerPrefab;
    private Dictionary<EWindowType, GameObject> windowsMap = new Dictionary<EWindowType, GameObject>();

    private List<EWindowType>activeWindows = new List<EWindowType>();

    private Dictionary<int,GameObject> layers = new Dictionary<int,GameObject>();
    private void Start(){
        for(int i = 0; i < windows.Count; i++){
            var key = windows[i].key;
            var value = windows[i].value;
            windowsMap.Add(key,value);
        }
        DontDestroyOnLoad(this.gameObject);
    }


    public BaseWindow ShowWindow(EWindowType windowType, BaseWindowData windowData = null)
    {
        if (IsOpen(windowType))
        {
            return null;
        }
        GameObject obj = new GameObject();
        bool has = windowsMap.TryGetValue(windowType, out obj);
        if (!has)
        {
            Debug.LogError($"Have no prefab for windowType - {windowType}");
            return null;
        }
        activeWindows.Add(windowType);
        BaseWindow window = Instantiate(obj).GetComponent<BaseWindow>();
        int layerValue = window.OrderInSort;
        if(!layers.ContainsKey(layerValue)){
            GameObject layer = Instantiate(layerPrefab);
            layer.transform.SetParent(transform);
            layers.Add(layerValue,layer);
            layer.name = layerValue.ToString();
        }
        GameObject parent = new GameObject();
        layers.TryGetValue(layerValue,out parent);
        window.gameObject.transform.SetParent(this.transform);
        window.gameObject.transform.localScale = new Vector3(1,1,1);
        window.gameObject.transform.localPosition = new Vector3(0,0,0);
        window.PrepareWindowData(windowData);
        window.Show();
        window.gameObject.transform.localPosition = new Vector3(0,0,0);
        return window;
    }

    public void HideWindow(EWindowType windowType,bool foced = false)
    {
        if (IsOpen(windowType))
        {
            activeWindows.Remove(windowType);
        }
        else{
            Debug.LogError($"Try hide no opened window - {windowType}");
        }
    }

    public bool IsOpen(EWindowType windowType){
        return activeWindows.Contains(windowType);
    }

}
