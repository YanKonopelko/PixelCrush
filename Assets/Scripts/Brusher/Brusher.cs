using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brusher : MonoBehaviour
{
    [SerializeField] MeshRenderer[] circlesRenderer = new MeshRenderer[2];
    [SerializeField] MeshRenderer brusherStickRenderer = new MeshRenderer();
    [SerializeField] MeshFilter[] circlesFilters = new MeshFilter[2];
    [SerializeField] MeshFilter brusherStickFilter = new MeshFilter();

    public void UpdateFromCongif()
    {
        LevelConfig config = PlayerData.Instance.CurentLevelConfig;
        brusherStickRenderer.material.color = config.BrusherStickColor;
          for (int i = 0; i < circlesRenderer.Length; i++)
        {
            circlesRenderer[i].material.color = config.BrusherCircleColor;
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
