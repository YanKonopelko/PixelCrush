using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrusherRotation : MonoBehaviour
{
    public Transform targerPoint;
    [SerializeField] private float _rotationSpeed = 175;
    static public bool isSwitched = true;
    [SerializeField] private CameraController _camera;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem stompAnim;

    [SerializeField] private Transform stick;
    [SerializeField] private float circleSizeScaler;

    [SerializeField] private Vector2 brusherSizeScaler;
    public Transform[] _rotationObject;

    private Vector3 direction = Vector3.up;

    private Vector3 startPosition;

    private bool AnimationNow = false;
    public static BrusherRotation instance;
    private Vector3[] startRotObjPositions;
    private void Start()
    {
        instance =  this;
        targerPoint = _rotationObject[0];
        isSwitched = true;
        startPosition = transform.position;
        LevelCreator.Instance.OnStart += OnStart;
        startRotObjPositions = new Vector3[2] { _rotationObject[0].position, _rotationObject[1].position };
    }

    private void ChangeDirection()
    {
        isSwitched = !isSwitched;

        SwapPoints();
        if (!this.CheckFloorAtThePoint(targerPoint))
        {
            ReloadRot();
            LevelCreator.Instance.isLose = true;
            LevelCreator.Instance.Restart();
        }
    }
    private void SwapPoints()
    {
       if(AnimationNow) return;
        var pos = _rotationObject[0].position;
        //targerPoint = isSwitched?_rotationObject[0]:_rotationObject[1];
        _rotationObject[0].position = _rotationObject[1].position;
        _rotationObject[1].position = pos;
        trailRenderer.Clear();
        stompAnim.Play();
    }

    public void OnStart(){
        StartAnimation(0.3f);
    }

    public void ReloadRot()
    {
        FinishAnimation(0.3f);
        //targerPoint = _rotationObject[0];
        transform.localRotation = new Quaternion(0, 0, 0, 0);
        transform.position = startPosition;
        _rotationObject[0].position = startRotObjPositions[0];
        _rotationObject[1].position = startRotObjPositions[1];
        FinishAnimation(0.2f);
        isSwitched = true;
    }
    void Update()
    {
        if (LevelCreator.Instance.isLose) return;
        if (!LevelCreator.Instance.isStart) return;
        if (LevelCreator.Instance.IsFinish) return;


        if (Input.GetMouseButtonDown(0) || (Input.touchCount != 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            PointerEventData ped = new PointerEventData(null);
            ped.position = Input.mousePosition;
            var allHits = new List<RaycastResult>();
            foreach (var graphicsRaycaster in FindObjectsOfType<GraphicRaycaster>())
            {
                var hits = new List<RaycastResult>();
                graphicsRaycaster.Raycast(ped, hits);

                allHits.AddRange(hits);
            }

            if (allHits.Count == 0)
                ChangeDirection();

        }
        else if (Input.GetKeyDown("space"))
        {
            ChangeDirection();
        }
        transform.RotateAround(targerPoint.position, direction * (isSwitched ? 1 : -1), _rotationSpeed * Time.deltaTime);
    }

    public Vector2 StickSize
    {
        get
        {
            Vector2 size = new Vector2(brusherSizeScaler.x * stick.localScale.y, brusherSizeScaler.y * stick.localScale.z);
            return size;
        }
    }
    public Vector3 StickPosition
    {
        get
        {
            return stick.position;
        }
    }

    public float CircleSize
    {
        get
        {
            return _rotationObject[0].transform.localScale.x*circleSizeScaler;
        }
    }
    public Vector3[] CirclePositions
    {
        get
        {           
            return new Vector3[2] { _rotationObject[0].transform.position, _rotationObject[1].transform.position };
        }
    }

    public bool CheckFloorAtThePoint(Transform point)
    {
        RaycastHit hit;
        Debug.DrawRay(point.position, point.TransformDirection(Vector3.down),new Color(1,1,1));
        return Physics.Raycast(point.position, point.TransformDirection(Vector3.down), out hit, 5f);
    }
    public void StartAnimation(float animationDuration){
        AnimationNow = true;
        var Seq = DOTween.Sequence(); 
        Seq.Append(_rotationObject[1].DOLocalMoveX(6.4f, animationDuration));
        Seq.Join(stick.DOLocalMoveX(3.2f, animationDuration));
        Seq.Join(stick.DOScaleY(2, animationDuration));
        Seq.OnComplete(()=>{AnimationNow = false;});
    }
      public void FinishAnimation(float animationDuration){
        AnimationNow = true;
        var Seq = DOTween.Sequence(); 
        Seq.Append(_rotationObject[1].DOLocalMoveX(0f, animationDuration));
        Seq.Join(_rotationObject[0].DOLocalMoveX(0f, animationDuration));
        Seq.Join(stick.DOLocalMoveX(0f, animationDuration));
        Seq.Join(stick.DOScaleY(0, animationDuration));
        Seq.OnComplete(()=>{AnimationNow = false;});
    }
}
