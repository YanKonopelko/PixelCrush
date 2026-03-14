using UnityEngine;

public class SpikeMechanic : MonoBehaviour
{
    public float maxHeight = 1f;
    public float timeToGrow = 2f;
    public float timeToLower = 2f;
    
    private float timer = 0f;
    private bool isGrowing = true;
    
    private Vector3 startPos;
    private Vector3 targetPos;

    private void Start()
    {
        targetPos = transform.position;
        startPos = transform.position + Vector3.down * maxHeight;
        transform.position = startPos;
    }

    private void Update()
    {
        if (GameScene.Instance != null && !GameScene.Instance.isStart) return;

        timer += Time.deltaTime;
        if (isGrowing)
        {
            float t = timer / timeToGrow;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (timer >= timeToGrow)
            {
                timer = 0f;
                isGrowing = false;
            }
        }
        else
        {
            float t = timer / timeToLower;
            transform.position = Vector3.Lerp(targetPos, startPos, t);
            if (timer >= timeToLower)
            {
                timer = 0f;
                isGrowing = true;
            }
        }
    }
}
