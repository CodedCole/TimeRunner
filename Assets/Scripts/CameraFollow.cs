using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float _followSpeed;
    [SerializeField] private float _stopDistance;

    private Transform _target;

    void LateUpdate()
    {
        if ((_target.position - transform.position).sqrMagnitude > _stopDistance * _stopDistance)
            transform.Translate((_target.position.x - transform.position.x) * _followSpeed, (_target.position.y - transform.position.y) * _followSpeed, 0);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
