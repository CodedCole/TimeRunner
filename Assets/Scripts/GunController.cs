using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform _anchor;

    private Gun _gun;
    private Inventory _inventory;
    private bool _useSecondary;

    // Start is called before the first frame update
    void Start()
    {
        _gun = GetComponentInChildren<Gun>();
        _inventory = GetComponent<Inventory>();
        _gun.GunInstance = _inventory.primaryWeapon;
    }

    /// <summary>
    /// Aims the right vector of the gunAnchor towards the world position 'pos' 
    /// </summary>
    /// <param name="pos">world position to aim at</param>
    public void AimAtPos(Vector2 pos)
    {
        float tan = Mathf.Atan2(pos.y - _anchor.position.y, pos.x - _anchor.position.x);
        _anchor.localEulerAngles = Vector3.forward * tan * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Fires the active weapon
    /// </summary>
    public void Fire()
    {
        _gun.Fire();
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
        if (_useSecondary)
        {
            _gun.GunInstance = _inventory.secondaryWeapon;
            Debug.Log("Secondary");
        }
        else
        {
            _gun.GunInstance = _inventory.primaryWeapon;
            Debug.Log("Primary");
        }
    }
}
