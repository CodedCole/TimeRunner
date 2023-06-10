using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GunStats stats;

    private int mag;
    private float shotTimer;
    private float reloadTimer;

    private void Start()
    {
        mag = stats.magSize;
    }

    private void Update()
    {
        
    }
}
