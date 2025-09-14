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

    // Кешированные данные для оптимизации
    private readonly double radToAngle = Math.PI / 180;
    private Vector2[] cachedCornerOffsets = new Vector2[4];
    private Square cachedSquare;
    private PointInQuadrilateral.Point cachedCircle1;
    private PointInQuadrilateral.Point cachedCircle2;
    private float cachedRadius;
    private bool needsSquareRecalculation = true;
    private bool needsCircleRecalculation = true;
    private float lastAngle = float.MinValue;
    private Vector3 lastStickPosition = Vector3.negativeInfinity;
    private Vector2 lastStickSize = Vector2.negativeInfinity;
    private Vector3[] lastCirclePositions = new Vector3[2];
    private const float POSITION_THRESHOLD = 0.001f;
    private const float ANGLE_THRESHOLD = 0.01f;
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
        // YG2.InterstitialAdvShow();
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
        
        // Сброс кеша при создании нового уровня
        ResetCache();
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
        
        // Освобождаем нативные массивы
        if (pixelPositions.IsCreated)
            pixelPositions.Dispose();
        if (pixelsPainted.IsCreated)
            pixelsPainted.Dispose();
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
        pixelPositions = new NativeArray<Vector2>(TargetCount, Allocator.Persistent);
        pixelsPainted = new NativeArray<bool>(TargetCount, Allocator.Persistent);
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

    private void ResetCache()
    {
        needsSquareRecalculation = true;
        needsCircleRecalculation = true;
        lastAngle = float.MinValue;
        lastStickPosition = Vector3.negativeInfinity;
        lastStickSize = Vector2.negativeInfinity;
        lastCirclePositions[0] = Vector3.negativeInfinity;
        lastCirclePositions[1] = Vector3.negativeInfinity;
    }

    private void CalculateSquareCorners(float angle, Vector2 brusherStickSize, Vector3 stickPosition)
    {
        // Кешируем тригонометрические значения
        double cosAngle = Math.Cos(radToAngle * angle);
        double sinAngle = Math.Sin(radToAngle * angle);

        // Вычисляем углы квадрата один раз
        cachedCornerOffsets[0] = CalculateCornerOffset(-0.5f * brusherStickSize.x, +0.5f * brusherStickSize.y, cosAngle, sinAngle); // leftUp
        cachedCornerOffsets[1] = CalculateCornerOffset(+0.5f * brusherStickSize.x, +0.5f * brusherStickSize.y, cosAngle, sinAngle); // rightUp
        cachedCornerOffsets[2] = CalculateCornerOffset(-0.5f * brusherStickSize.x, -0.5f * brusherStickSize.y, cosAngle, sinAngle); // leftDown
        cachedCornerOffsets[3] = CalculateCornerOffset(+0.5f * brusherStickSize.x, -0.5f * brusherStickSize.y, cosAngle, sinAngle); // rightDown

        // Добавляем позицию стика
        Vector2 offset = new Vector2(stickPosition.x, stickPosition.z);
        for (int i = 0; i < 4; i++)
        {
            cachedCornerOffsets[i] += offset;
        }

        // Создаем квадрат с кешированными точками (порядок: leftUp, rightUp, leftDown, rightDown)
        cachedSquare = new Square(
            new PointInQuadrilateral.Point(cachedCornerOffsets[0].x, cachedCornerOffsets[0].y), // leftUp
            new PointInQuadrilateral.Point(cachedCornerOffsets[1].x, cachedCornerOffsets[1].y), // rightUp
            new PointInQuadrilateral.Point(cachedCornerOffsets[2].x, cachedCornerOffsets[2].y), // leftDown
            new PointInQuadrilateral.Point(cachedCornerOffsets[3].x, cachedCornerOffsets[3].y)  // rightDown
        );
        

        
         #if UNITY_EDITOR
        Debug.DrawLine(new Vector3(cachedCornerOffsets[2].x, 2, cachedCornerOffsets[2].y), new Vector3(cachedCornerOffsets[0].x, 2, cachedCornerOffsets[0].y));
        Debug.DrawLine(new Vector3(cachedCornerOffsets[0].x, 2, cachedCornerOffsets[0].y), new Vector3(cachedCornerOffsets[1].x, 2, cachedCornerOffsets[1].y));
        Debug.DrawLine(new Vector3(cachedCornerOffsets[1].x, 2, cachedCornerOffsets[1].y), new Vector3(cachedCornerOffsets[3].x, 2, cachedCornerOffsets[3].y));
        Debug.DrawLine(new Vector3(cachedCornerOffsets[3].x, 2, cachedCornerOffsets[3].y), new Vector3(cachedCornerOffsets[2].x, 2, cachedCornerOffsets[2].y));
        #endif
    }

    private Vector2 CalculateCornerOffset(float x, float y, double cosAngle, double sinAngle)
    {
        return new Vector2(
            (float)(x * cosAngle - y * sinAngle),
            (float)(x * sinAngle + y * cosAngle)
        );
    }

    public void Update()
    {
        if (!GameScene.Instance.isStart) return;

        float angle = brusherRotation.Angle * (BrusherRotation.isSwitched ? -1 : -1);
        var brusherStickSize = brusherRotation.StickSize;
        Vector3 stickPosition = brusherRotation.StickPosition;
        Vector3[] circlePositions = brusherRotation.CirclePositions;
        float circleSize = brusherRotation.CircleSize;

        // Проверяем, нужно ли пересчитывать квадрат
        if (needsSquareRecalculation || 
            Math.Abs(angle - lastAngle) > ANGLE_THRESHOLD || 
            Vector3.Distance(stickPosition, lastStickPosition) > POSITION_THRESHOLD ||
            Vector2.Distance(brusherStickSize, lastStickSize) > POSITION_THRESHOLD)
        {
            CalculateSquareCorners(angle, brusherStickSize, stickPosition);
            lastAngle = angle;
            lastStickPosition = stickPosition;
            lastStickSize = brusherStickSize;
            needsSquareRecalculation = false;
        }

        // Проверяем, нужно ли пересчитывать круги
        if (needsCircleRecalculation ||
            Vector3.Distance(circlePositions[0], lastCirclePositions[0]) > POSITION_THRESHOLD ||
            Vector3.Distance(circlePositions[1], lastCirclePositions[1]) > POSITION_THRESHOLD ||
            Math.Abs(circleSize - cachedRadius) > POSITION_THRESHOLD)
        {
            cachedCircle1 = new PointInQuadrilateral.Point(circlePositions[0].x, circlePositions[0].z);
            cachedCircle2 = new PointInQuadrilateral.Point(circlePositions[1].x, circlePositions[1].z);
            cachedRadius = circleSize;
            lastCirclePositions[0] = circlePositions[0];
            lastCirclePositions[1] = circlePositions[1];
            needsCircleRecalculation = false;
        }

        TriggerPixelJob job = new TriggerPixelJob()
        {
            pixelsPositions = this.pixelPositions,
            outPixelPainted = pixelsPainted,
            brusherPosition = new Vector2(0, 0),
            brusherSize = brusherStickSize,
            pixelSize = pixelSize,
            circle1 = cachedCircle1,
            circle2 = cachedCircle2,
            radius = cachedRadius,
            square = cachedSquare
        };
        this.handle = job.Schedule(TargetCount, 64); // Увеличил batch size для лучшей производительности

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

    private void OnDestroy()
    {
        // Освобождаем нативные массивы
        if (pixelPositions.IsCreated)
            pixelPositions.Dispose();
        if (pixelsPainted.IsCreated)
            pixelsPainted.Dispose();
    }
}