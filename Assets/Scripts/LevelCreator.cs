using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private Transform brusher;
    [SerializeField] private BrusherRotation brusherRotation;

    [SerializeField] private Vector2 pixelSize;

    [SerializeField] private PixelScript[] pixelScripts;

    [SerializeField] private Pool pool;

    [SerializeField] private Text debugText;



    public bool isStart;
    public bool isLose;
    public bool IsFinish;

    public static LevelCreator Instance;
    private Dictionary<Color, Material> InUseColors = new Dictionary<Color, Material>();
    private List<GameObject> pixels = new List<GameObject>();
    private List<List<Vector3>> pixelsGrid = new List<List<Vector3>>();
    private NativeArray<Vector2> pixelPositions;


    public Action OnStart;
    public Action OnLose;
    public Action OnFinish;

    private int TargetCount = 0;
    private int CurrentCount = 0;

    private NativeArray<bool> pixelsPainted;
    private JobHandle handle;

    private void Start()
    {
        Pool.Instance = this.pool;
        LevelCreator.Instance = this;
        pool.PreparePool(particlePrefab, 50);
        AsyncCreateLevel();
        DOTween.SetTweensCapacity(200, 250);
        // Application.targetFrameRate = 50;
    }

    private async void AsyncCreateLevel()
    {
        int targetLevel;
        if(PlayerData.Instance.IsMaxLevelNow()){

            System.Random rnd = new System.Random();
            targetLevel = rnd.Next(0,PlayerData.LevelsCount);
            if(PlayerData.Instance.LastLevel != -1)
                targetLevel = PlayerData.Instance.LastLevel;
        }
        else{
            targetLevel = PlayerData.Instance.CurrentLevel;
        }
        PlayerData.Instance.LastLevel = targetLevel;
        Debug.Log(targetLevel);
        UniTask<Texture2D> textureTask = GlobalData.Instance.GetLevelTexture(targetLevel);
        texture2D = await textureTask;
        CreateLevel(texture2D);
        PlayerData.Instance.Save();
    }

    private void CreateLevel(Texture2D img)
    {
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
                if (color.a < 0.8) continue;
                bool firstInRow = true;
                bool firstInColumn = true;

                if (x > 0)
                {
                    Color previousColor = pixelData[x - 1 + y * width];
                    firstInRow = previousColor.a < 0.8;
                }

                if (y > 0)
                {
                    Color previousColor = pixelData[x + (y - 1) * width];
                    firstInColumn = previousColor.a < 0.8;
                }

                this.CreatPixel(new Vector3(pos.x, 0, pos.y), color, firstInColumn, firstInRow);
            }
        }
        TargetCount = pixels.Count;
        pixelPositions = new NativeArray<Vector2>(TargetCount, Allocator.TempJob);
        pixelsPainted = new NativeArray<bool>(TargetCount, Allocator.TempJob);
        pixelScripts = new PixelScript[TargetCount];
        for (int i = 0; i < TargetCount; i++)
        {
            pixelPositions[i] = new Vector2(pixels[i].transform.position.x, pixels[i].transform.position.z);
            pixelsPainted[i] = false;
            pixelScripts[i] = pixels[i].GetComponent<PixelScript>();
        }

    }

    private void CreatPixel(Vector3 pos, Color color, bool isFront = false, bool isRight = false)
    {
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
        pixel.name = pos.x.ToString() + "." + pos.z.ToString();
        pixel.transform.SetParent(pixelParent);
        pixel.transform.localPosition = pos;

        pixel.GetComponent<PixelScript>().rgbScaleMaterial = targetMaterial;
        pixel.GetComponent<PixelScript>().InitPixel(particlePrefab, OnPaint, basePixelMaterial, new bool[3] { true, isFront, isRight });
        pixels.Add(pixel);
        if (pixelsGrid.Count <= pos.z)
        {
            pixelsGrid.Add(new List<Vector3>());
        }
        pixelsGrid[(int)pos.z].Add(pos);
    }
    double radToAngle = Math.PI / 180;
    public void Update()
    {
        debugText.text = "Level: " + ( (PlayerData.Instance.AdditionalIndex>-1? PlayerData.Instance.CurrentLevel + PlayerData.Instance.AdditionalIndex:PlayerData.Instance.CurrentLevel)+1).ToString();
        if (!isStart) return;

        // a     b
        // c     d
        float angle = brusher.rotation.eulerAngles.y * (BrusherRotation.isSwitched ? -1 : -1);
        var brusherStickSize = brusherRotation.StickSize;
        float x = -0.5f * brusherStickSize.x;
        float y = +0.5f * brusherStickSize.y;
        Vector3 Offset = brusherRotation.StickPosition;
        Vector2 leftUp = new Vector2((float)(x * Math.Cos(radToAngle * angle) - y * Math.Sin(radToAngle * angle)), (float)(x * Math.Sin(radToAngle * angle) + y * Math.Cos(radToAngle * angle)));

        x = 0.5f * brusherStickSize.x;
        y = 0.5f * brusherStickSize.y;
        Vector2 rightUp = new Vector2((float)(x * Math.Cos(radToAngle * angle) - y * Math.Sin(radToAngle * angle)), (float)(x * Math.Sin(radToAngle * angle) + y * Math.Cos(radToAngle * angle)));

        x = -0.5f * brusherStickSize.x;
        y = -0.5f * brusherStickSize.y;
        Vector2 leftDown = new Vector2((float)(x * Math.Cos(radToAngle * angle) - y * Math.Sin(radToAngle * angle)), (float)(x * Math.Sin(radToAngle * angle) + y * Math.Cos(radToAngle * angle)));

        x = +0.5f * brusherStickSize.x;
        y = -0.5f * brusherStickSize.y;
        Vector2 rightDown = new Vector2((float)(x * Math.Cos(radToAngle * angle) - y * Math.Sin(radToAngle * angle)), (float)(x * Math.Sin(radToAngle * angle) + y * Math.Cos(radToAngle * angle)));

        leftUp += new Vector2(Offset.x, Offset.z);
        rightUp += new Vector2(Offset.x, Offset.z);
        leftDown += new Vector2(Offset.x, Offset.z);
        rightDown += new Vector2(Offset.x, Offset.z);
        Debug.DrawLine(new Vector3(leftDown.x, 2, leftDown.y), new Vector3(leftUp.x, 2, leftUp.y));
        Debug.DrawLine(new Vector3(leftUp.x, 2, leftUp.y), new Vector3(rightUp.x, 2, rightUp.y));
        Debug.DrawLine(new Vector3(rightUp.x, 2, rightUp.y), new Vector3(rightDown.x, 2, rightDown.y));
        Debug.DrawLine(new Vector3(rightDown.x, 2, rightDown.y), new Vector3(leftDown.x, 2, leftDown.y));

        var circle1Pos = new PointInQuadrilateral.Point(brusherRotation.CirclePositions[0].x, brusherRotation.CirclePositions[0].z);
        var circle2Pos = new PointInQuadrilateral.Point(brusherRotation.CirclePositions[1].x, brusherRotation.CirclePositions[1].z);

        TriggerPixelJob job = new TriggerPixelJob()
        {
            pixelsPositions = this.pixelPositions,
            outPixelPainted = pixelsPainted,
            brusherPosition = new Vector2(0, 0),
            brusherSize = brusherStickSize,
            pixelSize = pixelSize,
            circle1 = circle1Pos,
            circle2 = circle2Pos,
            radius = brusherRotation.CircleSize,
            square = new Square(new PointInQuadrilateral.Point(leftUp.x, leftUp.y), new PointInQuadrilateral.Point(rightUp.x, rightUp.y), new PointInQuadrilateral.Point(leftDown.x, leftDown.y), new PointInQuadrilateral.Point(rightDown.x, rightDown.y))
        };
        this.handle = job.Schedule(TargetCount, 5);

        this.handle.Complete();
        TriggerPixels();
    }

    private void TriggerPixels()
    {
        for (int i = 0; i < TargetCount; i++)
        {
            if (pixelsPainted[i])
            {
                pixelScripts[i].Paint();
            }
        }
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
        PlayerData.Instance.MarkLevelComplete();
        brusherRotation.ReloadRot();
         isStart = false;
        isLose = false;
        IsFinish = false;
        AsyncCreateLevel();
        StartCanvas.SetActive(true);
        GlobalData.Instance.UnloadLevelTexture(PlayerData.Instance.LastLevel);
        PlayerData.Instance.LastLevel = -1;
        PlayerData.Instance.Save();
    }
    public void Restart()
    {
        isStart = false;
        isLose = false;
        IsFinish = false;
        CreateLevel(texture2D);
        StartCanvas.SetActive(true);
    }

    public void StatGame()
    {
        isStart = true;
        OnStart?.Invoke();
        StartCanvas.SetActive(false);
    }
}