using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameLevelConfig))]
public class GameLevelConfigEditor : Editor
{
    private const float MAX_GRID_SIZE = 300f; // Максимальный размер сетки в пикселях
    private const float MIN_CELL_SIZE = 4f; // Минимальный размер ячейки
    private const float MAX_CELL_SIZE = 20f; // Максимальный размер ячейки
    private const float CELL_SPACING = 1f;
    
    public override void OnInspectorGUI()
    {
        GameLevelConfig config = (GameLevelConfig)target;
        
        // Получаем приватные поля через SerializedProperty
        SerializedProperty heightProp = serializedObject.FindProperty("_height");
        SerializedProperty widthProp = serializedObject.FindProperty("_width");
        SerializedProperty pixelsProp = serializedObject.FindProperty("_pixels");
        SerializedProperty coinsProp = serializedObject.FindProperty("_coins");
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Game Level Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Отображаем стандартные поля
        EditorGUILayout.PropertyField(heightProp, new GUIContent("Height"));
        EditorGUILayout.PropertyField(widthProp, new GUIContent("Width"));
        EditorGUILayout.PropertyField(pixelsProp, new GUIContent("Pixels"), true);
        EditorGUILayout.PropertyField(coinsProp, new GUIContent("Coins"), true);
        
        EditorGUILayout.Space();
        
        // Отображаем сетку
        if (config != null)
        {
            DrawPixelGrid(config);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private float CalculateCellSize(int width, int height)
    {
        // Вычисляем размер ячейки так, чтобы сетка помещалась в MAX_GRID_SIZE
        float maxDimension = Mathf.Max(width, height);
        float cellSize = MAX_GRID_SIZE / maxDimension;
        
        // Ограничиваем размер ячейки минимальными и максимальными значениями
        cellSize = Mathf.Clamp(cellSize, MIN_CELL_SIZE, MAX_CELL_SIZE);
        
        return cellSize;
    }
    
    private void DrawPixelGrid(GameLevelConfig config)
    {
        EditorGUILayout.LabelField("Pixel Grid Preview", EditorStyles.boldLabel);
        
        // Получаем размеры через рефлексию
        var heightField = typeof(GameLevelConfig).GetField("_height", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var widthField = typeof(GameLevelConfig).GetField("_width", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pixelsField = typeof(GameLevelConfig).GetField("_pixels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (heightField == null || widthField == null || pixelsField == null)
        {
            EditorGUILayout.HelpBox("Cannot access private fields", MessageType.Error);
            return;
        }
        
        int height = (int)heightField.GetValue(config);
        int width = (int)widthField.GetValue(config);
        List<PixelData> pixels = (List<PixelData>)pixelsField.GetValue(config);
        
        if (height <= 0 || width <= 0)
        {
            EditorGUILayout.HelpBox("Invalid grid dimensions", MessageType.Warning);
            return;
        }
        
        // Вычисляем адаптивный размер ячейки
        float cellSize = CalculateCellSize(width, height);
        int cellPixelSize = Mathf.RoundToInt(cellSize);
        int cellStep = Mathf.RoundToInt(cellSize + CELL_SPACING);
        
        // Создаем словарь для быстрого поиска пикселей по позиции
        Dictionary<Vector2, Color> pixelColors = new Dictionary<Vector2, Color>();
        if (pixels != null)
        {
            foreach (var pixel in pixels)
            {
                if (ColorUtility.TryParseHtmlString(pixel._colorHex, out Color color))
                {
                    pixelColors[pixel._pos] = color;
                }
                else
                {
                    Debug.LogWarning($"Failed to parse color: {pixel._colorHex} at position {pixel._pos}");
                }
            }
        }
        
        // Создаем множество позиций монеток для быстрого поиска
        HashSet<Vector2> coinPositions = new HashSet<Vector2>();
        if (config.Coins != null)
        {
            foreach (var coin in config.Coins)
            {
                coinPositions.Add(coin);
            }
        }
        
        // Создаем текстуру для всей сетки (используем целочисленный шаг, чтобы избежать накопления ошибки)
        int textureWidth = width * cellStep - Mathf.RoundToInt(CELL_SPACING);
        int textureHeight = height * cellStep - Mathf.RoundToInt(CELL_SPACING);
        Texture2D gridTexture = new Texture2D(textureWidth, textureHeight);
        
        // Заполняем фон
        Color[] texturePixels = new Color[textureWidth * textureHeight];
        Color defaultColor = new Color(0.412f, 0.412f, 0.412f, 1f); // #696969
        for (int i = 0; i < texturePixels.Length; i++)
        {
            texturePixels[i] = Color.gray; // Серый фон для сетки
        }
        gridTexture.SetPixels(texturePixels);
        
        // Рисуем ячейки
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Color cellColor = defaultColor; // По умолчанию #696969 (пустая ячейка)
                
                if (pixelColors.ContainsKey(pos))
                {
                    cellColor = pixelColors[pos];
                }
                
                // Вычисляем позицию ячейки в текстуре
                int startX = x * cellStep;
                int startY = (height - 1 - y) * cellStep; // Инвертируем Y для отображения
                
                // Заполняем ячейку цветом
                for (int py = 0; py < cellPixelSize; py++)
                {
                    for (int px = 0; px < cellPixelSize; px++)
                    {
                        int pixelX = startX + px;
                        int pixelY = startY + py;
                        if (pixelX < textureWidth && pixelY < textureHeight)
                        {
                            gridTexture.SetPixel(pixelX, pixelY, cellColor);
                        }
                    }
                }
            }
        }
        
        gridTexture.Apply();
        
        // Отображаем текстуру
        GUIStyle gridStyle = new GUIStyle();
        gridStyle.normal.background = gridTexture;
        gridStyle.fixedWidth = textureWidth;
        gridStyle.fixedHeight = textureHeight;
        
        // Получаем Rect для сетки
        Rect gridRect = GUILayoutUtility.GetRect(textureWidth, textureHeight, gridStyle);
        GUI.Label(gridRect, "", gridStyle);
        
        // Рисуем символы $ для монеток
        if (coinPositions.Count > 0)
        {
            GUIStyle coinStyle = new GUIStyle(EditorStyles.boldLabel);
            coinStyle.fontSize = Mathf.Max(8, Mathf.RoundToInt(cellPixelSize * 0.6f)); // Адаптивный размер шрифта
            coinStyle.normal.textColor = Color.yellow;
            coinStyle.alignment = TextAnchor.MiddleCenter;
            
            foreach (var coinPos in coinPositions)
            {
                int x = (int)coinPos.x;
                int y = (int)coinPos.y;
                
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    // Вычисляем точную позицию центра ячейки (используем тот же целочисленный шаг и инверсию Y, что и для пикселей)
                    float cellCenterX = gridRect.x + x * cellStep + cellPixelSize / 2f;
                    float cellCenterY = gridRect.y + (   y) * cellStep + cellPixelSize / 2f;
                    
                    // Создаем Rect точно по центру ячейки
                    Rect coinRect = new Rect(cellCenterX - cellPixelSize / 4f, cellCenterY - cellPixelSize / 4f, cellPixelSize / 2f, cellPixelSize / 2f);
                    GUI.Label(coinRect, "$", coinStyle);
                }
            }
        }
        
        // Показываем информацию о сетке
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Grid Size: {width} x {height}");
        EditorGUILayout.LabelField($"Cell Size: {cellSize:F1}px");
        EditorGUILayout.LabelField($"Pixels Count: {(pixels != null ? pixels.Count : 0)}");
        EditorGUILayout.LabelField($"Loaded Colors: {pixelColors.Count}");
        EditorGUILayout.LabelField($"Coins Count: {coinPositions.Count}");
    }
}
