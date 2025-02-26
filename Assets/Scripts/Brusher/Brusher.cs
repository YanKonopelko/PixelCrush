using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryNamespace;
using UnityEngine;

public class Brusher : MonoBehaviour
{
    [SerializeField] MeshRenderer[] circlesRenderer = new MeshRenderer[2];
    [SerializeField] MeshRenderer brusherStickRenderer = new MeshRenderer();
    [SerializeField] MeshFilter[] circlesFilters = new MeshFilter[2];
    [SerializeField] MeshFilter brusherStickFilter = new MeshFilter();

    public async Task UpdateFromInventory(){
        Item circle = await GlobalData.Instance.Inventory.EquipedCircleSkin();
        ChangeCirclesMesh(circle.Model);
        ChangeCirclesMaterial(circle.Material);
        Item stick = await GlobalData.Instance.Inventory.EquipedStickSkin();
        ChangeStickMesh(stick.Model);
        ChangeStickMaterial(stick.Material);
    }
    public void UpdateFromCongif()
    {
        LevelConfig config = PlayerData.Instance.CurentLevelConfig;
        ShaderColorPack pack = config.BrusherColor;
        brusherStickRenderer.material.SetColor("Color",pack.MainColor);
        brusherStickRenderer.material.SetColor("HighlightColor",pack.HighlightColor);
        brusherStickRenderer.material.SetColor("ShadowColor",pack.ShadowColor);
          for (int i = 0; i < circlesRenderer.Length; i++)
        {
            circlesRenderer[i].material.SetColor("Color",pack.MainColor);
            circlesRenderer[i].material.SetColor("HighlightColor",pack.HighlightColor);
            circlesRenderer[i].material.SetColor("ShadowColor",pack.ShadowColor);
        }
    }
    public void ChangeStickMesh(Mesh mesh)
    {
        brusherStickFilter.mesh = mesh;
    }
    public void ChangeStickMaterial(Material material)
    {
        brusherStickRenderer.material = material;
    }

    public void ChangeCirclesMesh(Mesh mesh)
    {
        for (int i = 0; i < circlesFilters.Length; i++)
        {
            circlesFilters[i].mesh = mesh;
        }
    }
    public void ChangeCirclesMaterial(Material material)
    {
        for (int i = 0; i < circlesRenderer.Length; i++)
        {
            circlesRenderer[i].material = material;
        }
    }

}
