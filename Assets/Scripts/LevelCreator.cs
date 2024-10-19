using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class LevelCreator : MonoBehaviour
{
    [SerializeField] private Texture2D texture2D;
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform pixelParent;
    [SerializeField] private Material pixelMaterial;
    [SerializeField] private Material sphereMaterial;
    // [SerializeField] private Material bigMeshMatereial;
    // [SerializeField] private MeshFilter filter;
    // [SerializeField] private MeshCollider bigMeshCollider;

    [SerializeField] private bool isCreate;

    [SerializeField] private Vector2 pixelSize;
    [SerializeField] private Pool pool;


    private Dictionary<Color, Material> InUseColors = new Dictionary<Color, Material>();
    private List<GameObject> pixels = new List<GameObject>();
    private List<List<Vector3>> pixelsGrid = new List<List<Vector3>>();

    private Mesh BigMesh;

    private void Start(){
        // pool.PreparePool(particlePrefab,20);
    }

    void Update()
    {
        if (isCreate)
        {
            isCreate = false;
            ClearChildren();
            // pixelsGrid.Add(new List<Vector3>());
            CreateLevelWithImage(texture2D);
            CreateBigMesh();
        }
    }
    public void ClearChildren()
    {
        for(int i =0; i < pixels.Count;i++){
            pool.Release(pixelPrefab,pixels[i].gameObject);
        }
        // while(pixelParent.childCount>0){
        //     DestroyImmediate(pixelParent.transform.GetChild(0).gameObject);
        // }
        pixels.Clear();
        pixelsGrid.Clear();
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

    }

    private void CreatPixel(Vector3 pos, Color color)
    {
        if (color.a < 0.8) return;
        // color.a = 1;
        // color.grayscale = 1;
        
        Material targetMaterial;
        Material grayMaterial;
        float gs = color.grayscale;

        if (!InUseColors.ContainsKey(color))
        {
            Material newMaterial = new Material(sphereMaterial);
            newMaterial.color = color;
            InUseColors.Add(color, newMaterial);
            // Debug.Log(newMaterial.color);
            Material newMaterialGray = new Material(sphereMaterial);
            newMaterialGray.color = new Color(gs,gs,gs);
            InUseColors.Add(new Color(gs,gs,gs), newMaterialGray);
        }
        
        InUseColors.TryGetValue(color, out targetMaterial);
        InUseColors.TryGetValue(new Color(gs,gs,gs), out grayMaterial);
        
        GameObject pixel = pool.GetFromPool(this.pixelPrefab);
        // GameObject pixel = Instantiate(this.pixelPrefab);
        
        pixel.transform.SetParent(pixelParent);
        pixel.transform.position = pos;
        pixel.GetComponent<MeshRenderer>().material = pixelMaterial;
        // pix
        // pixel.GetComponent<PixelScript>().grayScaleMaterial = grayMaterial;
        pixel.GetComponent<PixelScript>().rgbScaleMaterial = targetMaterial;
        pixel.GetComponent<PixelScript>().sphereRenderer.material = sphereMaterial;
        pixel.GetComponent<PixelScript>().InitPixel(particlePrefab);
        pixels.Add(pixel);
        if (pixelsGrid.Count <= pos.z)
        {
            pixelsGrid.Add(new List<Vector3>());
        }
        pixelsGrid[(int)pos.z].Add(pos);
        // pixelsPositions.Add(pixel.transform.position);
    }

    private void CreateBigMesh()
    {
        // Debug.Log(pixels.Count);

        // Mesh mesh = new Mesh();
        // Vector3[] Verticles = new Vector3[pixels.Count * 4];
        // Vector2[] uv = new Vector2[pixels.Count * 4];
        // int[] triangles = new int[pixels.Count * 6];

        // int Count = 0;
        // float height = pixelsGrid.Count;
        // Debug.Log(height);

        // for (int i = 0; i < pixelsGrid.Count; i++)
        // {
        //     List<Vector3> line = pixelsGrid[i];
        //     float width = line.Count;
        //     for (int j = 0; j < width; j++)
        //     {
        //         Vector3 pos = line[j];
        //         pos.y += 0.5f;
        //         Verticles[Count * 4] = pos;
        //         Verticles[Count * 4].x -= pixelSize.x;
        //         Verticles[Count * 4].z += pixelSize.y;
        //         // Сдвигать на относительный размер пискеля в углы
        //         Vector2 point = new Vector2(j / (width - 1), i / (height - 1));
        //         // - +
        //         // + +
        //         // - -
        //         // + -
        //         uv[Count * 4] = new Vector2((j-0.5f) / (width - 1), (i+0.5f) / (height - 1));
        //         uv[Count * 4 + 1] = new Vector2((j+0.5f) / (width - 1), (i+0.5f) / (height - 1));;
        //         uv[Count * 4 + 2] = new Vector2((j-0.5f) / (width - 1), (i-0.5f) / (height - 1));;
        //         uv[Count * 4 + 3] = new Vector2((j+0.5f) / (width - 1), (i-0.5f) / (height - 1));;
        //         // Debug.Log("UV's");
        //         // Debug.Log(uv[Count * 4]);
        //         // Debug.Log(uv[Count * 4 + 1]);
        //         // Debug.Log(uv[Count * 4 + 2]);
        //         // Debug.Log(uv[Count * 4 + 3]);
        //         Verticles[Count * 4 + 1] = pos;
        //         Verticles[Count * 4 + 1].x += pixelSize.x;
        //         Verticles[Count * 4 + 1].z += pixelSize.y;




        //         Verticles[Count * 4 + 2] = pos;
        //         Verticles[Count * 4 + 2].x -= pixelSize.x;
        //         Verticles[Count * 4 + 2].z -= pixelSize.y;



        //         Verticles[Count * 4 + 3] = pos;
        //         Verticles[Count * 4 + 3].x += pixelSize.x;
        //         Verticles[Count * 4 + 3].z -= pixelSize.y;



        //         triangles[Count * 6] = Count * 4;
        //         triangles[Count * 6 + 1] = Count * 4 + 1;
        //         triangles[Count * 6 + 2] = Count * 4 + 2;
        //         triangles[Count * 6 + 3] = Count * 4 + 2;
        //         triangles[Count * 6 + 4] = Count * 4 + 1;
        //         triangles[Count * 6 + 5] = Count * 4 + 3;

        //         // uv[Count * 4].x = Verticles[Count * 4].x;
        //         // uv[Count * 4].y = Verticles[Count * 4].z;
        //         // uv[Count * 4 + 1].x = Verticles[Count * 4 + 1].x;
        //         // uv[Count * 4 + 1].y = Verticles[Count * 4 + 1].z;
        //         // uv[Count * 4 + 2].x = Verticles[Count * 4 + 2].x;
        //         // uv[Count * 4 + 2].y = Verticles[Count * 4 + 2].z;
        //         // uv[Count * 4 + 3].x = Verticles[Count * 4 + 3].x;
        //         // uv[Count * 4 + 3].y = Verticles[Count * 4 + 3].z;

        //         Count++;
        //     }


        // }
        // // uv[0] = new Vector2(0,1);
        // // uv[0] = new Vector2(0,1);
        // // uv[0] = new Vector2(0,1);
        // // uv[0] = new Vector2(0,1);
        // mesh.vertices = Verticles;
        // mesh.uv = uv;
        // mesh.triangles = triangles;
        // mesh.Optimize();
        // BigMesh = mesh;
        // bigMeshCollider.sharedMesh = null;
        // bigMeshCollider.sharedMesh = mesh;
        // filter.mesh = mesh;
    }
}