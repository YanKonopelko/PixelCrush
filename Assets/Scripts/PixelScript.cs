
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TerrainTools;

public class PixelScript : MonoBehaviour
{
    [SerializeField] MeshRenderer renderer;
    [SerializeField] GameObject ParticleSystemKey;
    [SerializeField] public GameObject sphereObj;
    [SerializeField] public Animation sphereAnim;
    [SerializeField] public Collider coll;
    public Action paintCallback;

    public Material rgbScaleMaterial;    

    private bool isPainted = false;
    private Sequence sequence = null;
    private void OnTriggerEnter(Collider other)
    {
        if(isPainted) return;
        
        if(other.CompareTag("Sizer_0")){
            SphereResize(1.1f);
        }
        else if(other.CompareTag("Sizer_1")){
            SphereResize(1.3f);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(isPainted) return;
            SphereResize(1);
    }

    public async void Paint(){
        if(isPainted) return;
        isPainted =  true;
         if(sequence != null){
            sequence.Kill();
        }
        sequence = DOTween.Sequence();
        var myCallback = new TweenCallback(()=>DisableSphere());
        sequence.Append(sphereObj.transform.DOScale(new Vector3(0,0,0),0.1f).SetEase(Ease.InOutCirc)).OnComplete(myCallback);
        renderer.material = rgbScaleMaterial;
        paintCallback();
        this.tag = "Pixel_Disabled";
    }

    private void DisableSphere(){
        sphereObj.SetActive(false);
    }

     public void InitPixel(GameObject paticlePrefabKey,Action callback,Material startMat){
        renderer.material = startMat;
        paintCallback = callback;
        ParticleSystemKey = paticlePrefabKey;
        isPainted = false;
        sphereObj.SetActive(true);
        coll.enabled = true;
        this.tag = "Pixel";
        if(sequence != null){
            sequence.Kill();
        }
        sphereObj.transform.localScale = new Vector3(1,1,1);

    }

    private void SphereResize(float size){
         if(sequence != null){
            sequence.Kill();
        }
        sequence = DOTween.Sequence();
        sequence.Append(sphereObj.transform.DOScale(new Vector3(size,size,size),0.08f));
        // sphereObj.transform.localScale = ;
    }

}
