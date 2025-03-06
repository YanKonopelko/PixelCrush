using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 175;
    [SerializeField] private float flySpeed = 175;
    private Vector3 direction = Vector3.up;
    private int lifeTime = 1500;
    private GameObject prefab = null;

    async Task OnEnable()
    {
       await UniTask.Delay(lifeTime);
       GlobalData.Instance.pool.Release(prefab,gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.position, direction, _rotationSpeed * Time.deltaTime);
        transform.position += new Vector3(0,flySpeed*Time.deltaTime,0);
    }
    public void SetPrefab(GameObject prefab){
        this.prefab = prefab;
    }
}
