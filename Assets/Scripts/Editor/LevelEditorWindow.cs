using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelEditorWindow : EditorWindow
{
    private enum EditorTool
    {
        PaintPixel,
        ErasePixel,
        SetStartPoint,
        PlaceCoin,
        EraseCoin,
        PlaceSpike,
        PlaceBarrier,
        PlaceMovingObstacle,
        SelectMechanic
    }

    private GameLevelConfig currentConfig;
    private Texture2D baseTexture;
    
    private EditorTool currentTool = EditorTool.PaintPixel;
    
    // Zoom & Pan
    private Vector2 scrollPosition;
    private float zoom = 20f;
    private const float MIN_ZOOM = 5f;
    private const float MAX_ZOOM = 50f;

    // Temporary data while editing
    private int gridWidth = 10;
    private int gridHeight = 10;
    private Dictionary<Vector2, string> pixels = new Dictionary<Vector2, string>();
    private HashSet<Vector2> coins = new HashSet<Vector2>();
    private Vector2 startPosition = Vector2.zero;
    private float brusherSpeed = 1f;

    private List<SpikeData> spikes = new List<SpikeData>();
    private List<ElectricBarrierData> barriers = new List<ElectricBarrierData>();
    private List<MovingObstacleData> movingObstacles = new List<MovingObstacleData>();

    private int selectedMechanicIndex = -1;
    private string selectedMechanicType = ""; // "Spike", "Barrier", "MovingObstacle"

    [MenuItem("PixelCrush/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        try
        {
            DrawSidebar();
            DrawGrid();
        }
        catch (System.Exception ex)
        {
            // End horizontal group to keep GUILayout state consistent
            GUILayout.EndHorizontal();
            Debug.LogError("LevelEditorWindow error: " + ex.Message + "\n" + ex.StackTrace);
            return;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawSidebar()
    {
        GUILayout.BeginVertical(GUILayout.Width(250), GUILayout.ExpandHeight(true));
        
        GUILayout.Label("Level Config", EditorStyles.boldLabel);
        GameLevelConfig newConfig = (GameLevelConfig)EditorGUILayout.ObjectField(currentConfig, typeof(GameLevelConfig), false);
        if (newConfig != currentConfig)
        {
            currentConfig = newConfig;
            if (currentConfig != null) LoadFromConfig();
        }

        if (GUILayout.Button("Save Config") && currentConfig != null)
        {
            SaveToConfig();
        }

        if (GUILayout.Button("Create New Config"))
        {
            CreateNewConfig();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Generation", EditorStyles.boldLabel);
        baseTexture = (Texture2D)EditorGUILayout.ObjectField("Base Texture", baseTexture, typeof(Texture2D), false);
        if (GUILayout.Button("Generate from Texture") && baseTexture != null)
        {
            GenerateFromTexture();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Height", gridHeight);
        brusherSpeed = EditorGUILayout.FloatField("Brusher Speed", brusherSpeed);

        EditorGUILayout.Space();
        GUILayout.Label("Tools", EditorStyles.boldLabel);
        string[] toolNames = System.Enum.GetNames(typeof(EditorTool));
        currentTool = (EditorTool)GUILayout.SelectionGrid((int)currentTool, toolNames, 2);

        EditorGUILayout.Space();
        GUILayout.Label("View", EditorStyles.boldLabel);
        zoom = EditorGUILayout.Slider("Zoom", zoom, MIN_ZOOM, MAX_ZOOM);

        EditorGUILayout.Space();
        DrawMechanicProperties();

        GUILayout.EndVertical();
        
        // Separator
        GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
    }

    private void DrawMechanicProperties()
    {
        if (currentTool != EditorTool.SelectMechanic || selectedMechanicIndex == -1) return;

        GUILayout.Label("Selected Mechanic Properties", EditorStyles.boldLabel);

        if (selectedMechanicType == "Spike" && selectedMechanicIndex < spikes.Count)
        {
            SpikeData s = spikes[selectedMechanicIndex];
            s.position = EditorGUILayout.Vector2Field("Position", s.position);
            s.maxHeight = EditorGUILayout.FloatField("Max Height", s.maxHeight);
            s.timeToGrow = EditorGUILayout.FloatField("Time To Grow", s.timeToGrow);
            s.timeToLower = EditorGUILayout.FloatField("Time To Lower", s.timeToLower);
            spikes[selectedMechanicIndex] = s;
            
            if (GUILayout.Button("Delete Spike"))
            {
                spikes.RemoveAt(selectedMechanicIndex);
                selectedMechanicIndex = -1;
            }
        }
        else if (selectedMechanicType == "Barrier" && selectedMechanicIndex < barriers.Count)
        {
            ElectricBarrierData b = barriers[selectedMechanicIndex];
            b.startPosition = EditorGUILayout.Vector2Field("Start Position", b.startPosition);
            b.endPosition = EditorGUILayout.Vector2Field("End Position", b.endPosition);
            b.activeTime = EditorGUILayout.FloatField("Active Time", b.activeTime);
            b.inactiveTime = EditorGUILayout.FloatField("Inactive Time", b.inactiveTime);
            barriers[selectedMechanicIndex] = b;
            
            if (GUILayout.Button("Delete Barrier"))
            {
                barriers.RemoveAt(selectedMechanicIndex);
                selectedMechanicIndex = -1;
            }
        }
        else if (selectedMechanicType == "MovingObstacle" && selectedMechanicIndex < movingObstacles.Count)
        {
            MovingObstacleData m = movingObstacles[selectedMechanicIndex];
            m.startPosition = EditorGUILayout.Vector2Field("Start Position", m.startPosition);
            m.length = EditorGUILayout.FloatField("Length", m.length);
            m.speed = EditorGUILayout.FloatField("Speed", m.speed);
            m.axis = (MovingObstacleAxis)EditorGUILayout.EnumPopup("Axis", m.axis);
            m.movementRange = EditorGUILayout.FloatField("Movement Range", m.movementRange);
            movingObstacles[selectedMechanicIndex] = m;
            
            if (GUILayout.Button("Delete Obstacle"))
            {
                movingObstacles.RemoveAt(selectedMechanicIndex);
                selectedMechanicIndex = -1;
            }
        }
    }

    private void DrawGrid()
    {
        Rect gridRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.Box(gridRect, "");

        if (Event.current.type == EventType.ScrollWheel && gridRect.Contains(Event.current.mousePosition))
        {
            zoom -= Event.current.delta.y;
            zoom = Mathf.Clamp(zoom, MIN_ZOOM, MAX_ZOOM);
            Event.current.Use();
        }

        Vector2 contentSize = new Vector2(gridWidth * zoom, gridHeight * zoom);
        scrollPosition = GUI.BeginScrollView(gridRect, scrollPosition, new Rect(0, 0, contentSize.x, contentSize.y));

        // Draw background grid lines
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        for (int x = 0; x <= gridWidth; x++)
            Handles.DrawLine(new Vector3(x * zoom, 0), new Vector3(x * zoom, gridHeight * zoom));
        for (int y = 0; y <= gridHeight; y++)
            Handles.DrawLine(new Vector3(0, y * zoom), new Vector3(gridWidth * zoom, y * zoom));

        // Draw pixels
        foreach (var kvp in pixels)
        {
            Vector2 pos = kvp.Key;
            string hex = kvp.Value;
            if (ColorUtility.TryParseHtmlString(hex, out Color col))
            {
                DrawCell(pos, col);
            }
            else
            {
                DrawCell(pos, Color.gray);
            }
        }

        // Draw start position
        DrawCell(startPosition, new Color(0f, 1f, 0f, 0.5f));
        GUI.Label(GetCellRect(startPosition), "S", GetLabelStyle(Color.black));

        // Draw coins
        foreach (var c in coins)
        {
            GUI.Label(GetCellRect(c), "$", GetLabelStyle(Color.yellow));
        }

        // Draw spikes
        for (int i = 0; i < spikes.Count; i++)
        {
            DrawMechanicMarker(spikes[i].position, "Spike", i == selectedMechanicIndex && selectedMechanicType == "Spike", Color.red);
        }

        // Draw barriers
        for (int i = 0; i < barriers.Count; i++)
        {
            Vector2 start = barriers[i].startPosition;
            Vector2 end = barriers[i].endPosition;
            DrawMechanicMarker(start, "Bar-S", i == selectedMechanicIndex && selectedMechanicType == "Barrier", Color.cyan);
            DrawMechanicMarker(end, "Bar-E", i == selectedMechanicIndex && selectedMechanicType == "Barrier", Color.cyan);
            Handles.color = Color.cyan;
            Handles.DrawLine(new Vector2(start.x * zoom + zoom/2, (gridHeight - 1 - start.y) * zoom + zoom/2), 
                             new Vector2(end.x * zoom + zoom/2, (gridHeight - 1 - end.y) * zoom + zoom/2));
        }

        // Draw moving obstacles
        for (int i = 0; i < movingObstacles.Count; i++)
        {
            DrawMechanicMarker(movingObstacles[i].startPosition, "Mov", i == selectedMechanicIndex && selectedMechanicType == "MovingObstacle", Color.magenta);
        }

        HandleMouseEvents();

        GUI.EndScrollView();
    }

    private void DrawCell(Vector2 pos, Color col)
    {
        EditorGUI.DrawRect(GetCellRect(pos), col);
    }
    
    private void DrawMechanicMarker(Vector2 pos, string label, bool isSelected, Color bgCol)
    {
        Rect r = GetCellRect(pos);
        if (isSelected)
        {
            EditorGUI.DrawRect(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), Color.white);
        }
        EditorGUI.DrawRect(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), bgCol);
        GUI.Label(r, label.Substring(0, Mathf.Min(3, label.Length)), GetLabelStyle(Color.white));
    }

    private Rect GetCellRect(Vector2 pos)
    {
        // Y is inverted for drawing (0 at top, height at bottom visually, or vice versa? 
        // Let's make y=0 bottom visually to match Unity coordinates usually, or y=0 top?
        // In the original GenerateFromTexture, it loops x,y from 0 to width,height. 
        // Meaning (0,0) is bottom left if it's Unity coordinates. We'll draw (0,0) at bottom-left.
        float drawY = gridHeight - 1 - pos.y;
        return new Rect(pos.x * zoom, drawY * zoom, zoom, zoom);
    }

    private GUIStyle GetLabelStyle(Color textCol)
    {
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = textCol;
        style.fontSize = Mathf.Max(8, (int)(zoom * 0.4f));
        return style;
    }

    private void HandleMouseEvents()
    {
        Event e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            Vector2 mousePos = e.mousePosition;
            int x = Mathf.FloorToInt(mousePos.x / zoom);
            int y = gridHeight - 1 - Mathf.FloorToInt(mousePos.y / zoom);

            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                Vector2 cellPos = new Vector2(x, y);
                HandleGridClick(cellPos, e.type == EventType.MouseDown);
                e.Use();
                Repaint();
            }
        }
    }

    private void HandleGridClick(Vector2 cellPos, bool isMouseDown)
    {
        switch (currentTool)
        {
            case EditorTool.PaintPixel:
                if (!pixels.ContainsKey(cellPos)) pixels[cellPos] = "#808080";
                break;
            case EditorTool.ErasePixel:
                pixels.Remove(cellPos);
                break;
            case EditorTool.SetStartPoint:
                if (isMouseDown) startPosition = cellPos;
                break;
            case EditorTool.PlaceCoin:
                if (isMouseDown) coins.Add(cellPos);
                break;
            case EditorTool.EraseCoin:
                if (isMouseDown) coins.Remove(cellPos);
                break;
            case EditorTool.PlaceSpike:
                if (isMouseDown)
                {
                    spikes.Add(new SpikeData { position = cellPos, maxHeight = 1f, timeToGrow = 2f, timeToLower = 2f });
                    selectedMechanicIndex = spikes.Count - 1;
                    selectedMechanicType = "Spike";
                }
                break;
            case EditorTool.PlaceBarrier:
                if (isMouseDown)
                {
                    barriers.Add(new ElectricBarrierData { startPosition = cellPos, endPosition = cellPos + Vector2.right, activeTime = 3f, inactiveTime = 2f });
                    selectedMechanicIndex = barriers.Count - 1;
                    selectedMechanicType = "Barrier";
                }
                break;
            case EditorTool.PlaceMovingObstacle:
                if (isMouseDown)
                {
                    movingObstacles.Add(new MovingObstacleData { startPosition = cellPos, length = 1f, speed = 2f, axis = MovingObstacleAxis.X, movementRange = 5f });
                    selectedMechanicIndex = movingObstacles.Count - 1;
                    selectedMechanicType = "MovingObstacle";
                }
                break;
            case EditorTool.SelectMechanic:
                if (isMouseDown)
                {
                    SelectMechanicAt(cellPos);
                }
                break;
        }
    }

    private void SelectMechanicAt(Vector2 pos)
    {
        int sIdx = spikes.FindIndex(s => s.position == pos);
        if (sIdx != -1) { selectedMechanicIndex = sIdx; selectedMechanicType = "Spike"; return; }

        int bIdx = barriers.FindIndex(b => b.startPosition == pos || b.endPosition == pos);
        if (bIdx != -1) { selectedMechanicIndex = bIdx; selectedMechanicType = "Barrier"; return; }

        int mIdx = movingObstacles.FindIndex(m => m.startPosition == pos);
        if (mIdx != -1) { selectedMechanicIndex = mIdx; selectedMechanicType = "MovingObstacle"; return; }

        selectedMechanicIndex = -1;
        selectedMechanicType = "";
    }

    private void LoadFromConfig()
    {
        gridWidth = currentConfig.Width;
        gridHeight = currentConfig.Height;
        brusherSpeed = currentConfig.BrusherSpeed;
        startPosition = currentConfig.BrusherStartPosition;

        pixels.Clear();
        if (currentConfig.Pixels != null)
        {
            foreach (var p in currentConfig.Pixels)
            {
                pixels[p._pos] = p._colorHex;
            }
        }

        coins.Clear();
        if (currentConfig.Coins != null)
        {
            foreach (var c in currentConfig.Coins) coins.Add(c);
        }

        spikes = currentConfig.Spikes != null ? new List<SpikeData>(currentConfig.Spikes) : new List<SpikeData>();
        barriers = currentConfig.ElectricBarriers != null ? new List<ElectricBarrierData>(currentConfig.ElectricBarriers) : new List<ElectricBarrierData>();
        movingObstacles = currentConfig.MovingObstacles != null ? new List<MovingObstacleData>(currentConfig.MovingObstacles) : new List<MovingObstacleData>();

        selectedMechanicIndex = -1;
        Repaint();
    }

    private void SaveToConfig()
    {
        SerializedObject so = new SerializedObject(currentConfig);
        
        so.FindProperty("_width").intValue = gridWidth;
        so.FindProperty("_height").intValue = gridHeight;
        so.FindProperty("_brusherSpeed").floatValue = brusherSpeed;
        so.FindProperty("_brusherStartPosition").vector2Value = startPosition;

        SerializedProperty pxProp = so.FindProperty("_pixels");
        pxProp.ClearArray();
        int pxIndex = 0;
        foreach (var kvp in pixels)
        {
            pxProp.InsertArrayElementAtIndex(pxIndex);
            SerializedProperty elem = pxProp.GetArrayElementAtIndex(pxIndex);
            SerializedProperty posProp = elem.FindPropertyRelative("_pos");
            SerializedProperty hexProp = elem.FindPropertyRelative("_colorHex");
            if (posProp == null || hexProp == null)
            {
                Debug.LogError("SaveToConfig: Could not find '_pos' or '_colorHex' property on PixelData. Check PixelData serialized field names.");
                return;
            }
            posProp.vector2Value = kvp.Key;
            hexProp.stringValue = kvp.Value;
            pxIndex++;
        }

        SerializedProperty coinProp = so.FindProperty("_coins");
        coinProp.ClearArray();
        int cIndex = 0;
        foreach (var c in coins)
        {
            coinProp.InsertArrayElementAtIndex(cIndex);
            coinProp.GetArrayElementAtIndex(cIndex).vector2Value = c;
            cIndex++;
        }

        SerializedProperty sProp = so.FindProperty("_spikes");
        sProp.ClearArray();
        for (int i = 0; i < spikes.Count; i++)
        {
            sProp.InsertArrayElementAtIndex(i);
            SerializedProperty e = sProp.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("position").vector2Value = spikes[i].position;
            e.FindPropertyRelative("maxHeight").floatValue = spikes[i].maxHeight;
            e.FindPropertyRelative("timeToGrow").floatValue = spikes[i].timeToGrow;
            e.FindPropertyRelative("timeToLower").floatValue = spikes[i].timeToLower;
        }

        SerializedProperty bProp = so.FindProperty("_electricBarriers");
        bProp.ClearArray();
        for (int i = 0; i < barriers.Count; i++)
        {
            bProp.InsertArrayElementAtIndex(i);
            SerializedProperty e = bProp.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("startPosition").vector2Value = barriers[i].startPosition;
            e.FindPropertyRelative("endPosition").vector2Value = barriers[i].endPosition;
            e.FindPropertyRelative("activeTime").floatValue = barriers[i].activeTime;
            e.FindPropertyRelative("inactiveTime").floatValue = barriers[i].inactiveTime;
        }

        SerializedProperty mProp = so.FindProperty("_movingObstacles");
        mProp.ClearArray();
        for (int i = 0; i < movingObstacles.Count; i++)
        {
            mProp.InsertArrayElementAtIndex(i);
            SerializedProperty e = mProp.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("startPosition").vector2Value = movingObstacles[i].startPosition;
            e.FindPropertyRelative("length").floatValue = movingObstacles[i].length;
            e.FindPropertyRelative("speed").floatValue = movingObstacles[i].speed;
            e.FindPropertyRelative("axis").enumValueIndex = (int)movingObstacles[i].axis;
            e.FindPropertyRelative("movementRange").floatValue = movingObstacles[i].movementRange;
        }

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("Saved Level Config: " + currentConfig.name);
    }

    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Level Config", "NewLevelConfig", "asset", "Save config");
        if (string.IsNullOrEmpty(path)) return;

        GameLevelConfig newCfg = ScriptableObject.CreateInstance<GameLevelConfig>();
        AssetDatabase.CreateAsset(newCfg, path);
        currentConfig = newCfg;
        SaveToConfig();
    }

    private void GenerateFromTexture()
    {
        if (baseTexture == null) return;
        
        string path = AssetDatabase.GetAssetPath(baseTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        gridWidth = baseTexture.width;
        gridHeight = baseTexture.height;
        pixels.Clear();

        Color[] texPixels = baseTexture.GetPixels();
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Color c = texPixels[y * gridWidth + x];
                if (c.a >= 0.8f)
                {
                    c.a = 1f; // normalize alpha for hex
                    pixels[new Vector2(x, y)] = "#" + ColorUtility.ToHtmlStringRGB(c);
                }
            }
        }
        Repaint();
    }
}
