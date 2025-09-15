using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelRedactor : MonoBehaviour
{
    [SerializeField] private Texture2D texture2D;
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private float brusherSpeed = 1;
    [SerializeField] private Vector2 brusherStartPos = new Vector2(-1, -1);
    [SerializeField] private List<Vector2> coins = new List<Vector2>();
    [SerializeField] private Transform pixelsParent;
    [SerializeField] private GridLayoutGroup pixelsParentLayout;
    private PixelRedactorScript currentPixel;
    void Start()
    {
        SessionData.isFromLevelCreator = true;
        CreateImageFromTexture();
    }

    private void CreateImageFromTexture()
    {
        if (SessionData.CurrentConfig)
        {
        }
        else
        {
            SessionData.CurrentConfig = GetConfigFromImage();
        }


        var config = SessionData.CurrentConfig;
        if (!config) return;
        for (int i = pixelsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(pixelsParent.GetChild(i).gameObject);
        }
        brusherSpeed = config.BrusherSpeed;
        brusherStartPos = config.BrusherStartPosition;
        coins = config.Coins;
        for (int x = 0; x < config.Width; x++)
        {
            for (int y = 0; y < config.Height; y++)
            {
                Color color = new Color(0.412f, 0.412f, 0.412f, 1f); ;
                int idx = config.Pixels.FindIndex(t => t._pos.Equals(new Vector2(x, y)));
                if (idx != -1)
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString(config.Pixels[idx]._colorHex, out Color newColor))
                    {
                        color = newColor;
                    }

                }
                var pixel = Instantiate(pixelPrefab);
                pixel.transform.SetParent(pixelsParent);
                var component = pixel.GetComponent<PixelRedactorScript>();
                component.SetColor(color);
                component.UpdateByValue(0);
                component.index = new Vector2(x, y);
                component.action = () => { OnPixelTouch(component); };
                component.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        float scale = 10.5f / (float)Math.Max(config.Width, config.Height);
        pixelsParent.localScale = new Vector3(scale,scale,scale);
        pixelsParentLayout.constraintCount = config.Height;
    }

    private void OnPixelTouch(PixelRedactorScript pixel)
    {
        if (currentPixel)
        {
            currentPixel.Deselect();
        }
        currentPixel = pixel;
        Debug.Log($"OnTouch {pixel.index}");
    }
    public void ToGame()
    {
        SessionData.CurrentConfig = GetConfigFromImage();
        SceneManager.LoadScene("NewPixel4");
    }

    public void UpdateImage()
    {
        SessionData.CurrentConfig = null;
        CreateImageFromTexture();
    }

    public void Save()
    {
        if (!texture2D)
        {
            Debug.LogWarning("Have no texture for create!");
            return;
        }
#if UNITY_EDITOR

        GameLevelConfig asset = this.GetConfigFromImage();
        // asset.
        var time = TimeUtils.LocalNow();
        AssetDatabase.CreateAsset(asset, $"Assets/Data/NewLevels/GameLevelConfig_{time}.asset");
        AssetDatabase.SaveAssets();
        Debug.Log($"Add new level config config: GameLevelConfig_{time}");
#endif
    }

    private GameLevelConfig GetConfigFromImage()
    {
        List<PixelData> pixels = new List<PixelData>();
        int height = texture2D.height;
        int width = texture2D.width;
        var pixelData = texture2D.GetPixels32();
        Color newCol;
        UnityEngine.ColorUtility.TryParseHtmlString("htmlValue", out newCol);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Color color = pixelData[x + y * width];
                if (color.a < 0.8) continue;
                color.a = 1;
                var pixel = new PixelData(pos, "#" + color.ToHexString());
                pixels.Add(pixel);
            }
        }

        if (brusherStartPos == new Vector2(-1, -1))
            brusherStartPos = pixels[0]._pos;
        GameLevelConfig newAsset = new GameLevelConfig(texture2D.width, texture2D.height, pixels, coins, 1, brusherStartPos);
        return newAsset;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelRedactor))]
public class PartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelRedactor script = (LevelRedactor)target;
        if (GUILayout.Button("Update By Image"))
        {
            script.Save();
        }
    }
}
#endif