
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TerrainTools;

public class PixelScript : MonoBehaviour
{
    [SerializeField] GameObject ParticleSystemKey;
    [SerializeField] MeshRenderer[] corners;
    [SerializeField] public GameObject sphereObj;
    [SerializeField] public Animation sphereAnim;
    public Action paintCallback;

    public Material rgbScaleMaterial;    

    private bool isPainted = false;
    private Sequence sequence = null;

    // private void OnTriggerEnter(Collider other)
    // {
    //     if(isPainted) return;
    //       if(other.CompareTag("Brusher")){
    //        BrusherRotation.instance.SetCollidersEnable(true);
    //        Paint();
    //       }
    //     if(other.CompareTag("Sizer_0")){
    //         SphereResize(0.75f);
    //     }
    //     else if(other.CompareTag("Sizer_1")){
    //         SphereResize(0.65f);
    //     }
    // }
    // private void OnTriggerExit(Collider other)
    // {
    //     if(isPainted) {
    //         BrusherRotation.instance.SetCollidersEnable(false);
    //     }
    //     else{
    //         SphereResize(1);
    //     }
    // }

    public async void Paint(){
        if(isPainted) return;
        isPainted =  true;
         if(sequence != null){
            sequence.Kill();
        }
        GameObject psObject = Pool.Instance.GetFromPool(ParticleSystemKey);
        psObject.transform.position = sphereObj.transform.position;
        ParticleSystem ps = psObject.GetComponent<ParticleSystem>();
        // ps.startColor = rgbScaleMaterial.color;
        ps.Play();
        sequence = DOTween.Sequence();
        var myCallback = new TweenCallback(()=>DisableSphere());
        sequence.Append(sphereObj.transform.DOScale(new Vector3(0,0,0),0.1f).SetEase(Ease.InOutCirc)).OnComplete(myCallback);
        for(int i =0; i < corners.Length;i++){
            corners[i].material= rgbScaleMaterial;
        }
        paintCallback();
        this.tag = "Pixel_Disabled";
        await UniTask.Delay(500);
        Pool.Instance.Release(ParticleSystemKey,psObject);
    }

    private void DisableSphere(){
        sphereObj.SetActive(false);
    }

     public void InitPixel(GameObject paticlePrefabKey,Action callback,Material startMat,bool[] enabledCorners){
        for(int i =0; i < corners.Length;i++){
            corners[i].enabled = false;
            corners[i].material= startMat;
        }
        for(int i =0; i < enabledCorners.Length;i++){
            corners[i].enabled =enabledCorners[i];
        }
        paintCallback = callback;
        ParticleSystemKey = paticlePrefabKey;
        isPainted = false;
        sphereObj.SetActive(true);
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
        sequence.Append(sphereObj.transform.DOScale(new Vector3(1,1,size),0.08f));
        // sphereObj.transform.localScale = ;
    }

}
