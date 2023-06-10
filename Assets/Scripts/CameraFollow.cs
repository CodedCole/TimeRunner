using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float _followSpeed;
    [SerializeField] private Vector2 _maxPosition;
    [SerializeField] private Vector2 _minPosition;

    private Transform _target;
    private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.Translate((_target.position.x - transform.position.x) * _followSpeed, (_target.position.y - transform.position.y) * _followSpeed, 0);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
