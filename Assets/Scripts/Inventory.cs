using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private float _maxWeight;
    [SerializeField] private int _maxItems;
    [SerializeField] private List<Item> _startingItems;
    public GunItemInstance primaryWeapon { get; private set; }
    public GunItemInstance secondaryWeapon { get; private set; }
    public ArmorItemInstance helmet { get; private set; }
    public ArmorItemInstance bodyArmor { get; private set; }
    public ItemInstance leftGadget { get; private set; }
    public ItemInstance rightGadget { get; private set; }

    private Container _container;
    private Action onStart;
    private Action onEquip;
    public Action<GunItemInstance> onEquipWeapon;

    private void Start()
    {
        _container = new Container(_maxWeight, _maxItems);
        foreach(var i in _startingItems)
        {
            _container.AddItem(i.MakeItemInstance());
        }
        if (onStart != null)
            onStart();
    }

    public void RegisterOnStart(Action action) { onStart += action; }
    public void UnregisterOnStart(Action action) { onStart -= action; }

    public ref Container GetContainer()
    {
        return ref _container;
    }

    public ItemInstance GetGearSlot(EGearSlot gearSlot)
    {
        switch(gearSlot)
        {
            case EGearSlot.Helmet: return helmet;
            case EGearSlot.Body_Armor: return bodyArmor;
            case EGearSlot.Primary_Weapon: return primaryWeapon;
            case EGearSlot.Secondary_Weapon: return secondaryWeapon;
            case EGearSlot.Left_Gadget: return leftGadget;
            case EGearSlot.Right_Gadget: return rightGadget;
        }
        return null;
    }

    public bool EquipGear(ItemInstance item, EGearSlot gearSlot)
    {
        if (gearSlot == EGearSlot.Primary_Weapon || gearSlot == EGearSlot.Secondary_Weapon)
        {
            if (item is GunItemInstance)
            {
                if (gearSlot == EGearSlot.Primary_Weapon)
                    primaryWeapon = item as GunItemInstance;
                else
                    secondaryWeapon = item as GunItemInstance;
                Debug.Log("Equipped");
                if (onEquip != null)
                    onEquip();
                if (onEquipWeapon != null)
                    onEquipWeapon(item as GunItemInstance);

                return true;
            }
            else
                return false;
        }
        else if (gearSlot == EGearSlot.Helmet || gearSlot == EGearSlot.Body_Armor)
        {
            if (item is ArmorItemInstance)
            {
                if (gearSlot == EGearSlot.Helmet && ((item as ArmorItemInstance).item as ArmorItem).GetArmorType() == EArmorType.Helmet)
                    helmet = item as ArmorItemInstance;
                else
                    bodyArmor = item as ArmorItemInstance;
                Debug.Log("Equipped");
                if (onEquip != null)
                    onEquip();

                return true;
            }
            else
                return false;
        }
        else if (gearSlot == EGearSlot.Left_Gadget || gearSlot == EGearSlot.Right_Gadget)
        {
            if (item is GunItemInstance || item is ArmorItemInstance)
            {
                return false;
            }
            else
            {
                if (gearSlot == EGearSlot.Left_Gadget)
                    leftGadget = item;
                else
                    rightGadget = item;
                Debug.Log("Equipped");
                if (onEquip != null)
                    onEquip();

                return true;
            }
        }
        return false;
    }

    public void RegisterOnEquip(Action action) { onEquip += action; }
    public void UnregisterOnEquip(Action action) { onEquip -= action; }
}
