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
    private int _shotsFired;
    private bool _triggerPulled;

    /// <summary>
    /// Action called when reload is finished. Use to put the correct amount of ammo in the gun.
    /// </summary>
    public Action onReloaded;

    public delegate bool CanReload();

    /// <summary>
    /// Delegate that returns a bool which lets the gun initiate a reload
    /// </summary>
    public CanReload canReload;

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
            else
                _gunInstance.mag = _gunInstance.gun.GetStats().magSize;

            _reloading = false;
        }

        if (!_triggerPulled)
        {
            _shotsFired = 0;
        }
        else
        {
            _triggerPulled = false;
        }
    }

    public void Fire()
    {
        _triggerPulled = true;
        if (_gunInstance != null && _nextShotTime <= Time.time && _gunInstance.mag > 0 && !_reloading && (_shotsFired < _gunInstance.gun.GetStats().shotsPerTriggerPull || _gunInstance.gun.GetStats().shotsPerTriggerPull == 0))
        {
            _nextShotTime = Time.time + (1f / _gunInstance.gun.GetStats().rateOfFire);
            _gunInstance.mag--;

            Projectile p = Instantiate(_gunInstance.gun.GetProjectile(), transform.position + (transform.right * _gunInstance.gun.GetProjectileSpawn().x) + (transform.up * _gunInstance.gun.GetProjectileSpawn().y), transform.rotation);
            p.damage = _gunInstance.gun.GetStats().damage;
            p.damageType = _gunInstance.gun.GetStats().damageType;
            p.GetComponent<Rigidbody2D>().velocity = transform.right * _gunInstance.gun.GetStats().projectileVelocity;

            if (_gunInstance.mag <= 0)
                Reload();

            _shotsFired++;
        }
    }

    public void Reload()
    {
        if (_gunInstance != null && _gunInstance.mag < _gunInstance.gun.GetStats().magSize && (canReload == null || canReload()))
        {
            _reloading = true;
            _reloadedTime = Time.time + _gunInstance.gun.GetStats().reloadTime;
            Debug.Log("Start Reload");
        }
    }
}
