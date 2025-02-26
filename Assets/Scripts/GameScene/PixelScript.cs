
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

    [SerializeField] public GameObject TopObj;
    [SerializeField] public MeshRenderer topRend;
    [SerializeField] public MeshRenderer reverseBottomRend;
    [SerializeField] public GameObject reverseBottomObj;
    [SerializeField] public GameObject bottomRotatorParent;

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
        topRend.material.color = new Color(0,0,0);
        hasCoin = true;
        CoinCallback = callback;
    }
    public void SetCrosses(Action<Vector3> callback)
    {
        topRend.material.color = new Color(1,1,1);
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
        sequence.Append(TopObj.transform.DOScale(new Vector3(0, 0, 0), 0.1f).SetEase(Ease.InOutCirc)).OnComplete(myCallback);
        
        CoinCall();
        CrossesCall();

        //Needs to call after all because has await
        if (PlayerData.Instance.EffectsEnabled)
        {
            GameObject psObject = GlobalData.Instance.pool.GetFromPool(ParticleSystemKey);
            psObject.transform.position = TopObj.transform.position;
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
        TopObj.SetActive(false);
    }
    private Material topMaterial;
    public void InitPixel(GameObject paticlePrefabKey, Action callback, Material bottomStartMat, Material topStartMaterial, bool[] enabledCorners)
    {
        ResetEndAnim();
        topMaterial = topStartMaterial;
        reverseBottomRend.material = rgbScaleMaterial;
        paintCallback = callback;
        ParticleSystemKey = paticlePrefabKey;
        isPainted = false;
        TopObj.SetActive(true);
        for (int i = 0; i < bottoms.Length; i++)
        {
            bottoms[i].material = bottomStartMat;
        }

        TopObj.transform.localScale = new Vector3(1, 1, 1);

    }
    public void EndAnim(){
        float animationDuration = 3;
        reverseBottomObj.SetActive(true);
        var Seq = DOTween.Sequence();
        // Vector3 targetRotation = new Vector3(90,0,-90);
        // Vector3 targetRotation1 = new Vector3(-90,0,0);
        Vector3 targetRotation = new Vector3(180,90,0);
        Seq.Append(bottomRotatorParent.transform.DOScale(new Vector3(0.8f,0.8f,0.8f), animationDuration/3));
        Seq.Append(bottomRotatorParent.transform.DORotate(targetRotation, animationDuration/3));
        Seq.Append(bottomRotatorParent.transform.DOScale(1, animationDuration/3));
       
        // Seq.Append(bottoms[0].transform.DORotate(targetRotation, animationDuration));
        // Seq.Join(bottoms[0].transform.DOLocalMoveY(-0.007f, animationDuration));
        // Seq.Join(reverseBottomObj.transform.DOLocalMoveY(-0.2552394f, animationDuration));
        // Seq.Join(reverseBottomObj.transform.DORotate(targetRotation1, animationDuration));
    }
    private void ResetEndAnim(){
        reverseBottomObj.SetActive(false);
         Quaternion quat = new Quaternion();
        quat.eulerAngles = new Vector3(0,-180,0);
        bottomRotatorParent.transform.rotation = quat;
        bottomRotatorParent.transform.localScale = new Vector3(1,1,1);
        // Quaternion quat = new Quaternion();
        // quat.eulerAngles = new Vector3(90,0,-90);
        // reverseBottomObj.transform.rotation = quat;
        // reverseBottomObj.transform.localPosition = new Vector3(0,-0.007f,0);
        // Quaternion quat1 = new Quaternion();
        // quat1.eulerAngles = new Vector3(-90,0,180);
        // bottoms[0].transform.rotation = quat1;
        // bottoms[0].transform.localPosition = new Vector3(0,-0.2552394f,0);
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
