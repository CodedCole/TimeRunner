using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GunItem gunItem;

    private int _mag;
    private float _nextShotTime;
    private float _reloadedTime;

    private void Start()
    {
        _mag = gunItem.GetStats().magSize;
        _reloadedTime = Time.time;
        _nextShotTime = Time.time;
    }

    public void Fire()
    {
        if (_nextShotTime <= Time.time && _mag > 0)
        {
            _nextShotTime = Time.time + (1f / gunItem.GetStats().rateOfFire);
            _mag--;

            Projectile p = Instantiate(gunItem.GetProjectile(), transform.position + (transform.right * gunItem.GetProjectileSpawn().x) + (transform.up * gunItem.GetProjectileSpawn().y), transform.rotation);
            p.damage = gunItem.GetStats().damage;
            p.damageType = gunItem.GetStats().damageType;
            p.GetComponent<Rigidbody2D>().velocity = transform.right * gunItem.GetStats().projectileVelocity;
        }
    }
}
