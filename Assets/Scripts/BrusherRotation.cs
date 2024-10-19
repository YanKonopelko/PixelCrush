using UnityEngine;

public class BrusherRotation : MonoBehaviour
{
    // [SerializeField] Transform[] RotCenters;
    private Transform targerPoint;
    // Start is called before the first frame update
    [SerializeField] private float _rotationSpeed = 175;
    static public bool isSwitched = true;
    [SerializeField] private CameraController _camera; 
    public Transform[] _rotationObject; 

    // [SerializeField] public Vector2 localPoses;
    private Vector3 direction = Vector3.up;
    private float distance = 6.4f;


    private void Start()
    {
        targerPoint = _rotationObject[0];
        isSwitched = true;
        // distance = 6.4f;
        // _camera = Camera.main.gameObject;
        _camera.player = _rotationObject[0];

    }

    private void ChangeDirection()
    {
        // if(AnimationNow || CapsuleManager.isRecalc) return;
        isSwitched = !isSwitched;

        SwapPoints();
        // if (!this.CheckFloorAtThePoint(_rotationObject[0]))
        // {
        //     // AnimationNow = true;
        //     isSwitched = true;
        //     // LevelManager.instance.Reload();
        //     // return;
        //     // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // }
        // CapsuleManager.Instance.RecalcTargetCapsules();
    }
    private void SwapPoints()
    {
        var pos = _rotationObject[0].position;
        _rotationObject[0].position = _rotationObject[1].position;
        _rotationObject[1].position = pos;

        // Vector3 newPos = new Vector3()
        // {
        //     x = (isSwitched ? -1 : 1) * distance + pos.x
        // };
        // _rotationObject[0].localPosition = newPos;
        // _rotationObject[1].localPosition = pos;

    }

    public void ReloadRot()
    {
        // isRotate = false;
        _rotationObject[0].localPosition = new Vector3(0, 0, 0);
        _rotationObject[1].localPosition = new Vector3(6.4f, 0, 0);
        isSwitched = true;
        transform.localRotation = new Quaternion(0, 0, 0, 0);
    }
    void Update()
    {
        if (Input.GetKeyDown("k") || ( Input.touchCount!=0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            ChangeDirection();
        }
        // if(isRotate)
         transform.RotateAround(targerPoint.position, direction * (isSwitched?1:-1), _rotationSpeed * Time.deltaTime);
    }
    public bool CheckFloorAtThePoint(Transform point)
    {
        RaycastHit hit;
        Debug.DrawRay(point.position, point.TransformDirection(Vector3.down),new Color(1,1,1));
        return Physics.Raycast(point.position, point.TransformDirection(Vector3.down), out hit, 5f);
    }
}
