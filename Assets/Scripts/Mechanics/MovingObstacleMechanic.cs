using UnityEngine;

public class MovingObstacleMechanic : MonoBehaviour
{
    public float speed = 2f;
    public MovingObstacleAxis axis = MovingObstacleAxis.X;
    public float movementRange = 5f;
    
    private Vector3 startPosition;
    private int direction = 1;
    
    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (GameScene.Instance != null && !GameScene.Instance.isStart) return;

        Vector3 move = Vector3.zero;
        if (axis == MovingObstacleAxis.X) move.x = speed * direction * Time.deltaTime;
        else move.z = speed * direction * Time.deltaTime; // map Y map coordinate to Z world

        transform.position += move;
        
        float dist = axis == MovingObstacleAxis.X ? 
            Mathf.Abs(transform.position.x - startPosition.x) : 
            Mathf.Abs(transform.position.z - startPosition.z);
            
        if (dist >= movementRange)
        {
            // Reverse direction and clamp position to avoid drifting
            direction *= -1;
            if (axis == MovingObstacleAxis.X)
            {
                transform.position = new Vector3(startPosition.x + (direction == 1 ? 0 : movementRange), transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, startPosition.z + (direction == 1 ? 0 : movementRange));
            }
        }
    }
}
