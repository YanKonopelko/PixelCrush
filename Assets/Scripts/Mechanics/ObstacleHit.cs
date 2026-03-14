using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleHit : MonoBehaviour
{
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if hitting Brusher or BrusherRotation wrapper
        if (other.GetComponentInParent<Brusher>() != null || other.GetComponentInParent<BrusherRotation>() != null)
        {
            if (GameScene.Instance != null && !GameScene.Instance.isLose)
            {
                GameScene.Instance.isLose = true;
                GameScene.Instance.Restart();
            }
        }
    }
}
