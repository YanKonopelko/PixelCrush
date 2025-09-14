using UnityEngine;

[System.Serializable]
public struct ShaderColorPack
{
    [SerializeField] public Color MainColor;
    [SerializeField] public Color HighlightColor;
    [SerializeField] public Color ShadowColor;
}

[CreateAssetMenu(fileName = "VisualLevelConfig", menuName = "Scriptable Objects/VisualLevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    [SerializeField] private ShaderColorPack startTopMaterialColor;
    [SerializeField] private ShaderColorPack startBottomMaterialColor;
    [SerializeField] private ShaderColorPack brusherColor;

    [SerializeField] private Color fogColor;
    // 3 цвета на пиксель 
    // 3 цвета на низ
    // 3 цвета на палку 
    public ShaderColorPack StartTopMaterialColor
    {
        get { return startTopMaterialColor; }
    }
    public ShaderColorPack StartBottomMaterialColor
    {
        get { return startBottomMaterialColor; }
    }
    public Color FogColor
    {
        get { return fogColor; }
    }
    public ShaderColorPack BrusherColor
    {
        get { return brusherColor; }
    }
}
