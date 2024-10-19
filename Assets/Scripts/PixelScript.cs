
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TerrainTools;

public class PixelScript : MonoBehaviour
{
    [SerializeField] MeshRenderer renderer;
    [SerializeField] ParticleSystem ps;
    [SerializeField] ParticleSystem subPs;
    [SerializeField] public MeshRenderer sphereRenderer;
    public Material grayScaleMaterial;    
    public Material rgbScaleMaterial;    

    private bool isPainted = false;
    private void OnTriggerEnter(Collider other)
    {
       Paint();
    }

    private void Paint(){
        if(isPainted) return;
        ps.gameObject.SetActive(true);
        isPainted =  true;
        ps.startColor = rgbScaleMaterial.color;
        subPs.startColor = rgbScaleMaterial.color;
        ps.Play();
        sphereRenderer.gameObject.SetActive(false);
        renderer.material = rgbScaleMaterial;
    }
     public void InitPixel(GameObject paticlePrefabKey){
        isPainted = false;
        sphereRenderer.gameObject.SetActive(true);
    }
}
