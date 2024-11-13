using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [SerializeField] BrusherRotation rot;
    [SerializeField] Vector3 offset;
    private Transform _Thistransform;
    void Start(){
        _Thistransform = transform;
    }
    // Update is called once per frame
    void Update()
    {
        var pos = rot.targerPoint.position;
        pos += offset;
        _Thistransform.position = pos;
    }
}
