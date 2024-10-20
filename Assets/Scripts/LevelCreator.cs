using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// [ExecuteAlways]
public class LevelCreator : MonoBehaviour
{
    [SerializeField] private Texture2D texture2D;
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform pixelParent;
    [SerializeField] private Material paintedPixelMaterial;
    [SerializeField] private Material basePixelMaterial;
    [SerializeField] private Material sphereMaterial;
    [SerializeField] private GameObject StartCanvas;


    [SerializeField] private Pool pool;


    public bool isStart;
    public bool isLose;
    public bool IsFinish;

    public static LevelCreator Instance;
    private Dictionary<Color, Material> InUseColors = new Dictionary<Color, Material>();
    private List<GameObject> pixels = new List<GameObject>();
    private List<List<Vector3>> pixelsGrid = new List<List<Vector3>>();

    public Action OnStart;
    public Action OnLose;
    public Action OnFinish;

    private int TargetCount = 0;
    private int CurrentCount = 0;
    private void Start()
    {
        Pool.Instance = this.pool;
        LevelCreator.Instance = this;
        pool.PreparePool(particlePrefab, 50);
        CreateLevel(texture2D);
    }

    private void CreateLevel(Texture2D img){
        ClearChildren();
        CreateLevelWithImage(img);
    }

    public void ClearChildren()
    {
        for (int i = 0; i < pixels.Count; i++)
        {
            pool.Release(pixelPrefab, pixels[i].gameObject);
        }
        pixels.Clear();
        pixelsGrid.Clear();
        CurrentCount = 0;
    }
    private void CreateLevelWithImage(Texture2D texture)
    {
        int height = texture.height;
        int width = texture.width;
        var pixelData = texture.GetPixels();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Color color = pixelData[x + y * width];

                this.CreatPixel(new Vector3(pos.x, 0, pos.y), color);

            }
        }
        TargetCount = pixels.Count;
    }

    private void CreatPixel(Vector3 pos, Color color)
    {
        if (color.a < 0.8) return;
        Material targetMaterial;
        float gs = color.grayscale;

        if (!InUseColors.ContainsKey(color))
        {
            Material newMaterial = new Material(paintedPixelMaterial);
            newMaterial.color = color;
            InUseColors.Add(color, newMaterial);
        }

        InUseColors.TryGetValue(color, out targetMaterial);

        GameObject pixel = pool.GetFromPool(this.pixelPrefab);

        pixel.transform.SetParent(pixelParent);
        pixel.transform.position = pos;

        pixel.GetComponent<PixelScript>().rgbScaleMaterial = targetMaterial;
        pixel.GetComponent<PixelScript>().InitPixel(particlePrefab, OnPaint, basePixelMaterial);
        pixels.Add(pixel);
        if (pixelsGrid.Count <= pos.z)
        {
            pixelsGrid.Add(new List<Vector3>());
        }
        pixelsGrid[(int)pos.z].Add(pos);
    }

    private void OnPaint()
    {
        CurrentCount++;
        if (CurrentCount == TargetCount)
        {
            Win();
        }
    }

    private void Win()
    {
        IsFinish = true;
        Restart();
    }
    public void Restart(){
        isStart = false;
        isLose = false;
        IsFinish = false;
        CreateLevel(texture2D);
        StartCanvas.SetActive(true);

    }

    public void StatGame(){
        isStart = true;
        StartCanvas.SetActive(false);
    }
}