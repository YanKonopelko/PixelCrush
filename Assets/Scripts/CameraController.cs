using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    private Vector3 offset;

    [Range(0,1)]private float cameraSpeed = 0.042f;

    void Start()
    {
        offset = transform.position - player.position;
    }


    void Update()
    {
        Vector3 newPosition = new Vector3(offset.x + player.position.x, transform.position.y, offset.z + player.position.z);
        transform.position = Vector3.Lerp(transform.position, newPosition, cameraSpeed);
    }
}

