using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform _anchor;

    private Gun _gun;
    private Inventory _inventory;

    // Start is called before the first frame update
    void Start()
    {
        _gun = GetComponentInChildren<Gun>();
        _gun.GunInstance = null;
        _inventory = GetComponent<Inventory>();
        _inventory.onEquipWeapon += OnWeaponEquip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnWeaponEquip(GunItemInstance gunInstance)
    {
        _gun.GunInstance = gunInstance;
    }

    public void AimAtPos(Vector2 pos)
    {
        float tan = Mathf.Atan2(pos.y - _anchor.position.y, pos.x - _anchor.position.x);
        _anchor.localEulerAngles = Vector3.forward * tan * Mathf.Rad2Deg;
    }

    public void Fire()
    {
        _gun.Fire();
    }
}
