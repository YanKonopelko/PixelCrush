using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrusherRotation : MonoBehaviour
{
    private Transform targerPoint;
    [SerializeField] private float _rotationSpeed = 175;
    static public bool isSwitched = true;
    [SerializeField] private CameraController _camera;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Collider[] additionalColliders;
    [SerializeField] private ParticleSystem stompAnim;

    public Transform[] _rotationObject;

    private Vector3 direction = Vector3.up;

    private Vector3 startPosition;
    private Vector3[] startRotObjPositions;


    private void Awake()
    {
        targerPoint = _rotationObject[0];
        isSwitched = true;
        startPosition = transform.position;
        _camera.player = _rotationObject[0];
        startRotObjPositions = new Vector3[2]{_rotationObject[0].position,_rotationObject[1].position};

    }

    private void ChangeDirection()
    {
        isSwitched = !isSwitched;

        SwapPoints();
        if (!this.CheckFloorAtThePoint(_rotationObject[0]))
        {
            ReloadRot();
            LevelCreator.Instance.isLose = true;
            LevelCreator.Instance.Restart();
        }
    }
    private void SwapPoints()
    {
        stompAnim.Play();
        var pos = _rotationObject[0].position;
        _rotationObject[0].position = _rotationObject[1].position;
        _rotationObject[1].position = pos;
        trailRenderer.Clear();
    }

    public void ReloadRot()
    {
        transform.localRotation = new Quaternion(0, 0, 0, 0);
        transform.position = startPosition;
        _rotationObject[0].position = startRotObjPositions[0];
        _rotationObject[1].position = startRotObjPositions[1];
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
    public bool CheckFloorAtThePoint(Transform point)
    {
        RaycastHit hit;
        Debug.DrawRay(point.position, point.TransformDirection(Vector3.down),new Color(1,1,1));
        return Physics.Raycast(point.position, point.TransformDirection(Vector3.down), out hit, 5f);
    }

    private void SetCollidersEnable(bool isEnable){
        for(int i = 0; i < additionalColliders.Length; i++){
            additionalColliders[i].tag = isEnable?additionalColliders[i].name:"Disabled_Sizer";
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.tag);
        if(other.CompareTag("Pixel")){
           SetCollidersEnable(true);
           other.GetComponent<PixelScript>().Paint();
        //    SetCollidersEnable(false);
        }
    }
     private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Pixel_Disabled")){
           SetCollidersEnable(false);
        }
    }
}
