
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TerrainTools;

public class PixelScript : MonoBehaviour
{
    [SerializeField] MeshRenderer renderer;
    [SerializeField] GameObject ParticleSystemKey;
    // [SerializeField] ParticleSystem subPs;
    [SerializeField] public GameObject sphereObj;
    public Material grayScaleMaterial;    
    public Material rgbScaleMaterial;    

    private bool isPainted = false;
    private void OnTriggerEnter(Collider other)
    {
       Paint();
    }

    private async void Paint(){
        if(isPainted) return;
        GameObject psObject = Pool.Instance.GetFromPool(ParticleSystemKey);
        psObject.transform.position = sphereObj.transform.position;
        ParticleSystem ps = psObject.GetComponent<ParticleSystem>();
        isPainted =  true;
        ps.startColor = rgbScaleMaterial.color;
        ParticleSystem subPs = psObject.transform.GetChild(0).GetComponent<ParticleSystem>();
        subPs.startColor = rgbScaleMaterial.color;
        ps.Play();
        sphereObj.SetActive(false);
        renderer.material = rgbScaleMaterial;
        await Task.Delay((int)ps.main.duration*500);
        Pool.Instance.Release(ParticleSystemKey,psObject);
    }
     public void InitPixel(GameObject paticlePrefabKey){
        ParticleSystemKey = paticlePrefabKey;
        isPainted = false;
        sphereObj.SetActive(true);
    }
}
