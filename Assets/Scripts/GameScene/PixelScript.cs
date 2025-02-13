
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
    [SerializeField] MeshRenderer[] bottoms;

    [SerializeField] public GameObject sphereObj;
    [SerializeField] public MeshRenderer sphereRend;
    [SerializeField] public Animation sphereAnim;
    [SerializeField] public bool isTopColored;

    public Action paintCallback;

    public Material rgbScaleMaterial;

    private bool isPainted = false;
    private Sequence sequence = null;

    public async void Paint()
    {
        if (isPainted) return;
        GlobalData.Instance.SoundManager.PlaySound(ESoundType.PixelDisapearSound);
        isPainted = true;
        if (sequence != null)
        {
            sequence.Kill();
        }
        Vibrator.Vibrate(7);
        sequence = DOTween.Sequence();
        var myCallback = new TweenCallback(() => DisableSphere());
        sequence.Append(sphereObj.transform.DOScale(new Vector3(0, 0, 0), 0.1f).SetEase(Ease.InOutCirc)).OnComplete(myCallback);
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i].material = rgbScaleMaterial;
        }
        if (!isTopColored)
        {
            for (int i = 0; i < bottoms.Length; i++)
            {
                bottoms[i].material = rgbScaleMaterial;
            }
        }
        paintCallback();
        this.tag = "Pixel_Disabled";
        if(PlayerData.Instance.EffectsEnabled){
            GameObject psObject = GlobalData.Instance.pool.GetFromPool(ParticleSystemKey);
            psObject.transform.position = sphereObj.transform.position;
            ParticleSystem ps = psObject.GetComponent<ParticleSystem>();
            ps.Play();
            await UniTask.Delay(500);
            GlobalData.Instance.pool.Release(ParticleSystemKey, psObject);
        }
    }

    private void DisableSphere()
    {
        sphereObj.SetActive(false);
    }

    public void InitPixel(GameObject paticlePrefabKey, Action callback, Material bottomStartMat, Material topStartMaterial, bool[] enabledCorners)
    {
        if (!isTopColored)
        {
            sphereRend.material = topStartMaterial;
        }
        else
        {
            sphereRend.material = rgbScaleMaterial;
        }
        paintCallback = callback;
        ParticleSystemKey = paticlePrefabKey;
        isPainted = false;
        sphereObj.SetActive(true);
        for (int i = 0; i < bottoms.Length; i++)
        {
            bottoms[i].material = bottomStartMat;
        }
        this.tag = "Pixel";
        if (sequence != null)
        {
            sequence.Kill();
        }
        sphereObj.transform.localScale = new Vector3(1, 1, 1);

    }

    // private void SphereResize(float size)
    // {
    //     if (sequence != null)
    //     {
    //         sequence.Kill();
    //     }
    //     sequence = DOTween.Sequence();
    //     sequence.Append(sphereObj.transform.DOScale(new Vector3(1, 1, size), 0.08f));
    //     // sphereObj.transform.localScale = ;
    // }

}
