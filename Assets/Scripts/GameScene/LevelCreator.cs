using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using YG;

// [ExecuteAlways]
public class LevelCreator : MonoBehaviour
{
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform pixelParent;
    [SerializeField] private Material bottomMaterial;
    [SerializeField] private Material topMaterial;
    [SerializeField] private BrusherRotation brusherRotation;
    [SerializeField] private Brusher brusher;

    [SerializeField] private Vector2 pixelSize;

    [SerializeField] private PixelScript[] pixelScripts;
    [SerializeField] private Texture2D testTexture;


    [SerializeField] private List<MeshRenderer> fogMeshes = new List<MeshRenderer>();
    [SerializeField] private List<TutorialStage> tutorialStages = new List<TutorialStage>();
    [SerializeField] private GameObject CoinPrefab = null;

    private Texture2D texture2D;
    private Dictionary<Color, Material> InUseColors = new Dictionary<Color, Material>();
    private List<GameObject> pixels = new List<GameObject>();
    private List<List<Vector3>> pixelsGrid = new List<List<Vector3>>();
    private NativeArray<Vector2> pixelPositions;

    private int TargetCount = 0;
    public int CurrentCount {get; private set;}

    private NativeArray<bool> pixelsPainted;
    private JobHandle handle;
    private const int MaxCoins = 5;
    private int lastTutorialStep = 0;
    public async UniTask AsyncCreateLevel()
    {
        if (testTexture != null)
        {
            texture2D = testTexture;
            CreateLevel();
            return;
        }
        int targetLevel;
        if (PlayerData.Instance.IsMaxLevelNow())
        {

            if (PlayerData.Instance.LastLevel != -1)
                targetLevel = PlayerData.Instance.LastLevel;
            else
                targetLevel = PlayerData.Instance.GetLevelFromList();
        }
        else
        {
            targetLevel = PlayerData.Instance.CurrentLevel;
        }
        PlayerData.Instance.LastLevel = targetLevel;
        UniTask<Texture2D> textureTask = GlobalData.Instance.GetLevelTexture(targetLevel);
        texture2D = await textureTask;
        CreateLevel();
        PlayerData.Instance.Save();
    }

    public void CreateLevel()
    {
        YG2.InterstitialAdvShow();
        brusher.UpdateFromCongif();
        var config = PlayerData.Instance.CurentLevelConfig;
        ShaderColorPack topPack = config.StartTopMaterialColor;
        ShaderColorPack bottomPack = config.StartBottomMaterialColor;
        topMaterial.SetColor("Color", topPack.MainColor);
        topMaterial.SetColor("_HColor", topPack.HighlightColor);
        topMaterial.SetColor("_SColor", topPack.ShadowColor);
        bottomMaterial.SetColor("Color", bottomPack.MainColor);
        bottomMaterial.SetColor("_HColor", bottomPack.HighlightColor);
        bottomMaterial.SetColor("_SColor", bottomPack.ShadowColor);
        for (int i = 0; i < fogMeshes.Count; i++)
        {
            fogMeshes[i].material.color = config.FogColor;
        }
        ClearChildren();
        CreateLevelWithImage(texture2D);
        brusherRotation.gameObject.SetActive(true);
    }

    public void ClearChildren()
    {
        for (int i = 0; i < pixels.Count; i++)
        {
            GlobalData.Instance.pool.Release(pixelPrefab, pixels[i].gameObject);
        }
        pixels.Clear();
        pixelsGrid.Clear();
        CurrentCount = 0;
    }
    private void CreateLevelWithImage(Texture2D texture)
    {
        int height = texture.height;
        int width = texture.width;
        var pixelData = texture.GetPixels32();

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
        InitCoins();
        // InitCrosses();
    }

    private void InitCoins()
    {
        System.Random random = new System.Random();
        int coinsCount = random.Next(0, MaxCoins);
        for (int i = 0; i < coinsCount; i++)
        {
            pixelScripts[random.Next(0, pixelScripts.Length)].SetCoin(SpawnCoin);
        }
    }
    private void InitCrosses()
    {
        System.Random random = new System.Random();
        pixelScripts[random.Next(0, pixelScripts.Length)].SetCrosses(SpawnCrosses);

    }
    private void SpawnCrosses(Vector3 pos)
    {
        Debug.Log($"Spawn Crosses: {pos}");
    }
    private void SpawnCoin(Vector3 pos)
    {
        //Запустить партикл монетки вверх
        GameObject coin = GlobalData.Instance.pool.GetFromPool(CoinPrefab);
        coin.GetComponent<Coin>().SetPrefab(CoinPrefab);
        // Debug.Log($"Spawn Coin: {pos}");
        coin.transform.position = pos;
        PlayerData.Instance.AddCoins(1);
        YG2.SaveProgress();
    }
    private void CreatPixel(Vector3 pos, Color color, bool isFront = false, bool isRight = false)
    {
        Material targetMaterial;
        float gs = color.grayscale;
        if (!InUseColors.ContainsKey(color))
        {
            Material newMaterial = new Material(topMaterial);
            Color hColor = new Color(color.r*0.98f,color.g*0.98f,color.b*0.92f);
            Color sColor = new Color(color.r*0.6f,color.g*0.35f,color.b*0.1f);
            newMaterial.color = color;
            newMaterial.SetColor("Color", color);
            newMaterial.SetColor("_HColor", hColor);
            newMaterial.SetColor("_SColor", sColor);
            InUseColors.Add(color, newMaterial);
        }

        InUseColors.TryGetValue(color, out targetMaterial);
        GameObject pixel = GlobalData.Instance.pool.GetFromPool(this.pixelPrefab);
        pixel.name = pos.x.ToString() + "." + pos.z.ToString();
        pixel.transform.SetParent(pixelParent);
        pixel.transform.localPosition = new Vector3(pos.x * pixelSize.x, pos.y * pixelSize.y, pos.z * pixelSize.x);

        pixel.GetComponent<PixelScript>().rgbScaleMaterial = targetMaterial;
        pixel.GetComponent<PixelScript>().InitPixel(particlePrefab, OnPaint, bottomMaterial, topMaterial, new bool[3] { true, isFront, isRight });
        pixels.Add(pixel);
        while (pixelsGrid.Count <= pos.z)
        {
            pixelsGrid.Add(new List<Vector3>());
        }
        pixelsGrid[(int)pos.z].Add(pos);
    }
    double radToAngle = Math.PI / 180;
    public void Update()
    {
        if (!GameScene.Instance.isStart) return;

        // a     b
        // c     d
        float angle = brusherRotation.Angle * (BrusherRotation.isSwitched ? -1 : -1);
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
        // Debug.DrawLine(new Vector3(leftDown.x, 2, leftDown.y), new Vector3(leftUp.x, 2, leftUp.y));
        // Debug.DrawLine(new Vector3(leftUp.x, 2, leftUp.y), new Vector3(rightUp.x, 2, rightUp.y));
        // Debug.DrawLine(new Vector3(rightUp.x, 2, rightUp.y), new Vector3(rightDown.x, 2, rightDown.y));
        // Debug.DrawLine(new Vector3(rightDown.x, 2, rightDown.y), new Vector3(leftDown.x, 2, leftDown.y));

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
        if(!PlayerData.Instance.TutorialComplete)
            CheckForTutorialStep();
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
            brusherRotation.gameObject.SetActive(false);
            PixelsEndAnim();
            GameScene.Instance.Win();

        }
    }
    public void PixelsEndAnim()
    {
        for (int i = 0; i < pixelScripts.Length; i++)
        {
            pixelScripts[i].EndAnim();
        }
    }
    public Vector3 GetLevelCenter()
    {
        int height = texture2D.height;
        int width = texture2D.width;
        float x = width / 2.4f * pixelSize.x;
        float z = height / 5 * pixelSize.y;
        return new Vector3(x, 0.5f, z);
    }
    public NativeArray<Vector2> GetPoses(){
        return pixelPositions;
    }
    private void CheckForTutorialStep(){
        if(tutorialStages.Count<=lastTutorialStep) return;
        var stage = tutorialStages[lastTutorialStep];
        if(CurrentCount >= stage.targetPixelsCount && Math.Abs(brusherRotation.Angle - stage.targetAngle) <= 5){
            lastTutorialStep += 1;
            GameScene.Instance.isStart = false;
             TutorialWindowData windowData = new TutorialWindowData();
            windowData.HideCallback = () => {brusherRotation.ChangeDirection();GameScene.Instance.isStart = true;};
            windowData.Step = 0;
            GlobalData.Instance.UIManager.ShowWindow(EWindowType.TutorialWindow, windowData);
        }
    }
}