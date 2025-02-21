
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TerrainTools;
using YG;

public class PixelScript : MonoBehaviour
{
    [SerializeField] MeshRenderer[] bottoms;

    [SerializeField] public GameObject sphereObj;
    [SerializeField] public MeshRenderer sphereRend;

    public Action paintCallback;

    public Material rgbScaleMaterial;

    private Sequence sequence = null;
    GameObject ParticleSystemKey;
    Action<Vector3> CoinCallback;
    Action<Vector3> CrossesCallback;
    private bool isPainted = false;

    private bool hasCoin = false;
    private bool hasCrosses = false;

    public bool HasCoin { get { return hasCoin; } }
    public bool HasCrosses { get { return hasCrosses; } }
    public void SetCoin(Action<Vector3> callback)
    {
        sphereRend.material.color = new Color(0,0,0);
        hasCoin = true;
        CoinCallback = callback;
    }
    public void SetCrosses(Action<Vector3> callback)
    {
        sphereRend.material.color = new Color(1,1,1);
        hasCrosses = true;
        CrossesCallback = callback;
    }
    public async void Paint()
    {
        if (isPainted) return;
        paintCallback();
        GlobalData.Instance.SoundManager.PlaySound(ESoundType.PixelDisapearSound);
        isPainted = true;
        Vibrator.Vibrate(7);
        sequence = DOTween.Sequence();
        var myCallback = new TweenCallback(() => DisableSphere());
        sequence.Append(sphereObj.transform.DOScale(new Vector3(0, 0, 0), 0.1f).SetEase(Ease.InOutCirc)).OnComplete(myCallback);
        
        CoinCall();
        CrossesCall();

        //Needs to call after all because has await
        if (PlayerData.Instance.EffectsEnabled)
        {
            GameObject psObject = GlobalData.Instance.pool.GetFromPool(ParticleSystemKey);
            psObject.transform.position = sphereObj.transform.position;
            ParticleSystem ps = psObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = topMaterial.color;
            ps.Play();
            await UniTask.Delay(500);
            GlobalData.Instance.pool.Release(ParticleSystemKey, psObject);
        }
    }

    private void DisableSphere()
    {
        sphereObj.SetActive(false);
    }
    private Material topMaterial;
    public void InitPixel(GameObject paticlePrefabKey, Action callback, Material bottomStartMat, Material topStartMaterial, bool[] enabledCorners)
    {
        topMaterial = topStartMaterial;
        sphereRend.material = topStartMaterial;

        paintCallback = callback;
        ParticleSystemKey = paticlePrefabKey;
        isPainted = false;
        sphereObj.SetActive(true);
        for (int i = 0; i < bottoms.Length; i++)
        {
            bottoms[i].material = bottomStartMat;
        }

        sphereObj.transform.localScale = new Vector3(1, 1, 1);

    }
    private void CoinCall(){
        if(!hasCoin){
            return;
        }
        CoinCallback?.Invoke(transform.position);
    }
     private void CrossesCall(){
        if(!hasCrosses){
            return;
        }
        CrossesCallback?.Invoke(transform.position);
    }
}
