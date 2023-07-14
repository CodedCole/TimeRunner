using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

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
                _spriteRenderer.sprite = value.gun.GetSprite();
                _library.spriteLibraryAsset = value.gun.GetSpriteLibrary();
                _animator.runtimeAnimatorController = value.gun.GetAnimatorController();
                _reloading = false;
            }
            else
            {
                _gunInstance = null;
                _spriteRenderer.sprite = null;
                _animator.runtimeAnimatorController = null;
                _reloading = false;
            }
        } 
    }

    [SerializeField] private SpriteRenderer _spriteRenderer;

    private SpriteLibrary _library;
    private Animator _animator;
    private ParticleSystem _muzzleFlash;

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
        _library = _spriteRenderer.GetComponent<SpriteLibrary>();
        _animator = GetComponent<Animator>();
        _muzzleFlash = GetComponentInChildren<ParticleSystem>();

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

            if (_animator != null)
                _animator.speed = 1;

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

            _muzzleFlash.Play();

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
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetTrigger("Reload");
                _animator.speed = 1 / _gunInstance.gun.GetStats().reloadTime;
            }
            Debug.Log("Start Reload");
        }
    }

    public bool IsReloading()
    {
        return _reloading;
    }

    public void SetSpriteFlip(bool flip)
    {
        _spriteRenderer.flipY = flip;
    }

    public void SetSpriteSortingOrder(int order)
    {
        _spriteRenderer.sortingOrder = order;
    }
}
