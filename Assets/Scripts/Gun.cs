using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GunItemInstance GunInstance 
    { 
        get 
        { 
            return _gunInstance; 
        } 
        set 
        {
            if (value != null)
            {
                _gunInstance = value;
                _nextShotTime = Time.time;
                _reloadedTime = Time.time;
                spriteRenderer.sprite = value.gun.GetSprite();
                _reloading = false;
            }
            else
            {
                _gunInstance = null;
                spriteRenderer.sprite = null;
                _reloading = false;
            }
        } 
    }

    [SerializeField] private SpriteRenderer spriteRenderer;

    private GunItemInstance _gunInstance;
    private float _nextShotTime;
    private float _reloadedTime;
    private bool _reloading;

    /// <summary>
    /// Action called when reload is finished. Use to put the correct amount of ammo in the gun.
    /// </summary>
    public Action onReloaded;

    private void Start()
    {
        _reloadedTime = Time.time;
        _nextShotTime = Time.time;
    }

    private void Update()
    {
        if (_reloading && _reloadedTime <= Time.time)
        {
            if (onReloaded != null)
                onReloaded();
            //_gunInstance.mag = _gunInstance.gun.GetStats().magSize;
            _reloading = false;
        }
    }

    public void Fire()
    {
        if (_gunInstance != null && _nextShotTime <= Time.time && _gunInstance.mag > 0 && !_reloading)
        {
            _nextShotTime = Time.time + (1f / _gunInstance.gun.GetStats().rateOfFire);
            _gunInstance.mag--;

            Projectile p = Instantiate(_gunInstance.gun.GetProjectile(), transform.position + (transform.right * _gunInstance.gun.GetProjectileSpawn().x) + (transform.up * _gunInstance.gun.GetProjectileSpawn().y), transform.rotation);
            p.damage = _gunInstance.gun.GetStats().damage;
            p.damageType = _gunInstance.gun.GetStats().damageType;
            p.GetComponent<Rigidbody2D>().velocity = transform.right * _gunInstance.gun.GetStats().projectileVelocity;

            if (_gunInstance.mag <= 0)
                Reload();
        }
    }

    public void Reload()
    {
        if (_gunInstance != null)
        {
            _reloading = true;
            _reloadedTime = Time.time + _gunInstance.gun.GetStats().reloadTime;
        }
    }
}
