using UnityEngine;

// [System.Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelConfig", order = 1)]
public class LevelConfig:ScriptableObject
{
    [SerializeField] private Color startTopMaterialColor;
    // [SerializeField] private Color startBottomMaterialColor;
    [SerializeField] private Color fogColor;
    [SerializeField] private Color brusherStickColor;
    [SerializeField] private Color brusherCircleColor = UnityEngine.Color.white;

    public Color StartTopMaterialColor{
        get{return startTopMaterialColor;}
    }
    // public Color StartBottomMaterialColor{
    //     get{return startBottomMaterialColor;}
    // }
    public Color FogColor{
        get{return fogColor;}
    }
    public Color BrusherStickColor{
        get{return brusherStickColor;}
    }
    public Color BrusherCircleColor{
        get{return brusherCircleColor;}
    }
}
