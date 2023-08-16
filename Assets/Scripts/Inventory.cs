using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class Inventory : MonoBehaviour
{
    [SerializeField] private float _maxWeight;
    [SerializeField] private int _maxItems;
    [SerializeField] private List<ItemInitialState> _startingItems;
    [SerializeField] private SpriteLibrary _helmetLibrary;
    [SerializeField] private SpriteLibrary _bodyArmorLibrary;
    public GunItemInstance primaryWeapon { get; private set; }
    public GunItemInstance secondaryWeapon { get; private set; }
    public ArmorItemInstance helmet { get; private set; }
    public ArmorItemInstance bodyArmor { get; private set; }
    public GadgetItemInstance leftGadget { get; private set; }
    public GadgetItemInstance rightGadget { get; private set; }

    private Container _container;
    private RaidManager _raidManager;

    private Action onStart;
    private Action onEquip;
    public Action<GunItemInstance> onEquipWeapon;
    public bool Started { get; private set; } = false;

    private void Start()
    {
        _raidManager = FindAnyObjectByType<RaidManager>();
        _container = new Container(_maxWeight, _maxItems);
        foreach(var i in _startingItems)
        {
            ItemInstance ii = i.item.MakeItemInstance();
            ii.stack = i.stackSize;
            _container.MoveItemInstance(ii);
        }
        if (onStart != null)
            onStart();
        Started = true;
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
            if (item == null || item is GunItemInstance)
            {
                //equip gear
                GunItemInstance gun = item == null ? null : item as GunItemInstance;
                if (gearSlot == EGearSlot.Primary_Weapon)
                    primaryWeapon = gun;
                else
                    secondaryWeapon = gun;

                Debug.Log(gun == null ? "Removed" : "Equipped");

                //equip events
                if (onEquip != null)
                    onEquip();
                if (onEquipWeapon != null)
                    onEquipWeapon(gun);

                return true;
            }
        }
        else if (gearSlot == EGearSlot.Helmet || gearSlot == EGearSlot.Body_Armor)
        {
            if (item == null || item is ArmorItemInstance)
            {
                //equip gear
                ArmorItemInstance armor = item == null ? null : item as ArmorItemInstance;
                if (gearSlot == EGearSlot.Helmet && armor.armor.GetArmorType() == EArmorType.Helmet)
                {
                    helmet = armor;
                    _helmetLibrary.spriteLibraryAsset = helmet?.armor.GetSpriteLibrary();
                    _helmetLibrary.GetComponent<CustomSpriteResolver>().UpdateSprite();
                }
                else
                {
                    bodyArmor = armor;
                    _bodyArmorLibrary.spriteLibraryAsset = bodyArmor?.armor.GetSpriteLibrary();
                    _bodyArmorLibrary.GetComponent<CustomSpriteResolver>().UpdateSprite();
                }

                Debug.Log(armor == null ? "Removed" : "Equipped");

                //equip event
                if (onEquip != null)
                    onEquip();

                return true;
            }
        }
        else if (gearSlot == EGearSlot.Left_Gadget || gearSlot == EGearSlot.Right_Gadget)
        {
            if (item == null || item is GadgetItemInstance)
            {
                //equip gear
                GadgetItemInstance gadget = item == null ? null : item as GadgetItemInstance;
                if (gearSlot == EGearSlot.Left_Gadget)
                {
                    if (leftGadget != null)
                        leftGadget.OnGadgetConsumed -= OnGadgetConsumed;

                    leftGadget = gadget;
                }
                else
                {
                    if (rightGadget != null)
                        rightGadget.OnGadgetConsumed -= OnGadgetConsumed;

                    rightGadget = gadget;
                }
                if (gadget != null)
                    gadget.OnGadgetConsumed += OnGadgetConsumed;

                Debug.Log(gadget == null ? "Removed" : "Equipped");

                //equip event
                if (onEquip != null)
                    onEquip();

                return true;
            }
        }

        //invalid gear
        return false;
    }

    void OnGadgetConsumed(GadgetItemInstance gadget)
    {
        if (gadget.stack <= 0)
        {
            if (gadget == leftGadget)
            {
                leftGadget = null;
            }
            else if (gadget == rightGadget)
            {
                rightGadget = null;
            }
            else
            {
                Debug.LogError("gadget (" + gadget.gadget.GetItemName() + ") is not in a gadget slot but was consumed");
            }
            if (onEquip != null)
                onEquip();
        }
    }

    public void RegisterOnEquip(Action action) { onEquip += action; }
    public void UnregisterOnEquip(Action action) { onEquip -= action; }

    public ArmorStats GetCurrentArmorStats()
    {
        ArmorStats helm = new ArmorStats();
        if (helmet != null)
            helm = helmet.armor.GetStats();
        ArmorStats body = new ArmorStats();
        if (bodyArmor != null)
            body = bodyArmor.armor.GetStats();
        return ArmorStats.Combine(body, helm);
    }

    public void UseLeftGadget()
    {
        if (leftGadget != null)
            leftGadget.Use(this.gameObject);
    }

    public void UseRightGadget()
    {
        if (rightGadget != null)
            rightGadget.Use(this.gameObject);
    }
}
