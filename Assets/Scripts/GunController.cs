using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform _anchor;
    [SerializeField] private GunItem _defaultGun;

    private Gun _gun;
    private Inventory _inventory;
    private bool _useSecondary;

    private Vector2 _currentDirection;

    // Start is called before the first frame update
    void Start()
    {   
        _gun = GetComponentInChildren<Gun>();
        _inventory = GetComponent<Inventory>();
        if (_inventory != null)
        {
            _gun.GunInstance = _inventory.primaryWeapon;
            _inventory.RegisterOnEquip(UpdateUsedWeapon);
        }
        else
            _gun.GunInstance = _defaultGun.MakeItemInstance() as GunItemInstance;
        _gun.onReloaded += OnReloaded;
        _gun.canReload = CanReload;
    }

    /// <summary>
    /// Aims the right vector of the gunAnchor towards the world position 'pos' 
    /// </summary>
    /// <param name="pos">world position to aim at</param>
    public void AimAtPos(Vector2 pos)
    {
        _currentDirection = new Vector2(pos.x - _anchor.position.x, pos.y - _anchor.position.y);
        float tan = Mathf.Atan2(_currentDirection.y, _currentDirection.x);
        _anchor.localEulerAngles = Vector3.forward * tan * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Aims the right vactor of the gunAnchor in the direction of 'dir'
    /// </summary>
    /// <param name="dir">direction to aim in</param>
    public void AimInDirection(Vector2 dir)
    {
        AimAtPos((Vector2)_anchor.position + dir);
    }

    /// <summary>
    /// Fires the active weapon
    /// </summary>
    public void Fire()
    {
        if (_gun.GunInstance == null)
            return;

        _gun.Fire();
        if (_gun.GunInstance.mag <= 0)
            Reload();
    }

    /// <summary>
    /// Reloads the active weapon
    /// </summary>
    public void Reload()
    {
        if (_gun.IsReloading())
            return;
        _gun.Reload();
    }

    /// <summary>
    /// Switches the active weapon to either primary or secondary weapon
    /// </summary>
    /// <param name="secondary">whether to switch to the secondary weapon</param>
    public void SelectWeapon(bool secondary)
    {
        if (secondary == _useSecondary)
            return;
        
        _useSecondary = secondary;
        UpdateUsedWeapon();
    }

    /// <summary>
    /// Toggles whether the primary or secondary weapon is the active weapon
    /// </summary>
    public void SwapWeapons()
    {
        _useSecondary = !_useSecondary;
        UpdateUsedWeapon();
    }

    /// <summary>
    /// Updates the gun to use the proper gun instance depending on _useSecondary
    /// </summary>
    private void UpdateUsedWeapon()
    {
        if (_inventory == null)
            return;

        if (_useSecondary)
        {
            _gun.GunInstance = _inventory.secondaryWeapon;
        }
        else
        {
            _gun.GunInstance = _inventory.primaryWeapon;
        }
    }

    /// <summary>
    /// Reloads the weapon to the proper amount of ammo
    /// </summary>
    private void OnReloaded()
    {
        int absent = 0;
        if(_inventory != null)
            absent = _inventory.GetContainer().RemoveItem(_gun.GunInstance.gun.GetAmmo(), _gun.GunInstance.gun.GetStats().magSize - _gun.GunInstance.mag);
        _gun.GunInstance.mag = _gun.GunInstance.gun.GetStats().magSize - absent;
    }

    /// <summary>
    /// Lets the weapon start the reload sequence
    /// </summary>
    /// <returns>whether the weapon can reload</returns>
    private bool CanReload()
    {
        if (_inventory == null)
            return true;
        bool canReload = _inventory.GetContainer().ContainsItem(_gun.GunInstance.gun.GetAmmo());
        return canReload;
    }

    public Vector2 GetAimVector()
    {
        return _currentDirection;
    }
}
