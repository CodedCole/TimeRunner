using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private float _maxWeight;
    [SerializeField] private int _maxItems;
    [SerializeField] private List<Item> _startingItems;
    public GunItem primaryWeapon { get; private set; }
    public GunItem secondaryWeapon { get; private set; }
    public ArmorItem helmet { get; private set; }
    public ArmorItem bodyArmor { get; private set; }
    public Item leftGadget { get; private set; }
    public Item rightGadget { get; private set; }

    private Container _container;
    private Action onStart;
    private Action onEquip;

    private void Start()
    {
        _container = new Container(_maxWeight, _maxItems);
        foreach(var i in _startingItems)
        {
            _container.AddItem(i);
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

    public bool EquipGear(Item item, EGearSlot gearSlot)
    {
        if (gearSlot == EGearSlot.Primary_Weapon || gearSlot == EGearSlot.Secondary_Weapon)
        {
            if (item is GunItem)
            {
                if (gearSlot == EGearSlot.Primary_Weapon)
                    primaryWeapon = item as GunItem;
                else
                    secondaryWeapon = item as GunItem;
                Debug.Log("Equipped");
                if (onEquip != null)
                    onEquip();

                return true;
            }
            else
                return false;
        }
        else if (gearSlot == EGearSlot.Helmet || gearSlot == EGearSlot.Body_Armor)
        {
            if (item is ArmorItem)
            {
                if (gearSlot == EGearSlot.Helmet && (item as ArmorItem).GetArmorType() == EArmorType.Helmet)
                    helmet = item as ArmorItem;
                else
                    bodyArmor = item as ArmorItem;
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
            if (item is GunItem || item is ArmorItem)
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
