using UnityEngine;

public class ElectricBarrierMechanic : MonoBehaviour
{
    public float activeTime = 3f;
    public float inactiveTime = 2f;
    
    private float timer = 0f;
    private bool isActive = true;
    
    private MeshRenderer visualRenderer;
    private Collider barrierCollider;
    
    private void Start()
    {
        visualRenderer = GetComponent<MeshRenderer>();
        barrierCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (GameScene.Instance != null && !GameScene.Instance.isStart) return;

        timer += Time.deltaTime;
        float limit = isActive ? activeTime : inactiveTime;
        
        if (timer >= limit)
        {
            timer = 0f;
            isActive = !isActive;
            if (visualRenderer != null) visualRenderer.enabled = isActive;
            if (barrierCollider != null) barrierCollider.enabled = isActive;
        }
    }
}
