using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform LevelParent;
    [SerializeField] private Vector3 baseRot;

    [Range(0,1)]private float cameraSpeed = 0.042f;

    public bool FinishAnimNow = false;

    private Transform _transform;
    private float animationDuration = 3;

    private void Start(){
        _transform = transform;
    }

    void Update()
    {
        if(FinishAnimNow) return;
        Vector3 newPosition = new Vector3(offset.x + player.position.x,offset.y + player.position.y, offset.z + player.position.z);
        _transform.position = Vector3.Lerp(_transform.position, newPosition, cameraSpeed);
    }

    public async UniTask FinishAnim(){
        bool animEnd = false;
        FinishAnimNow = true;
        var Seq = DOTween.Sequence();
        Vector3 targetRotation = new Vector3(90,0,0);
        Vector3 targetPos = LevelParent.position;
        Seq.Append(_transform.DOLocalMoveX(targetPos.x+17, animationDuration));
        Seq.Join(_transform.DOLocalMoveY(130, animationDuration));
        Seq.Join(_transform.DOLocalMoveZ(targetPos.z+3, animationDuration));
        Seq.Join(_transform.DORotate(targetRotation, animationDuration));
        Seq.OnComplete(() => { animEnd = true; 
        });
        await UniTask.WaitUntil(()=> animEnd == true);
    }

    public void ToDefaultValues(){
        FinishAnimNow = false;
        _transform.rotation = Quaternion.Euler(baseRot);
    }
}

