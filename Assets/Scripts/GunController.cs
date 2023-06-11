using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform _anchor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AimAtPos(Vector2 pos)
    {
        float tan = Mathf.Atan2(pos.y - _anchor.position.y, pos.x - _anchor.position.x);
        _anchor.localEulerAngles = Vector3.forward * tan * Mathf.Rad2Deg;
    }
}
