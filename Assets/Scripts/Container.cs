using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    private float _maxWeight;
    private int _maxItems;
    
    private List<ItemInstance> _items = new List<ItemInstance>();
    private float _currentWeight = 0;

    Action onItemAdded;
    Action onItemRemoved;

    public Container(float maxWeight, int maxItems) 
    { 
        _maxWeight = maxWeight;
        _maxItems = maxItems;
    }

    /// <summary>
    /// Moves an ItemInstance without copying the data or trying to stack. Use with non-stackable items like weapons and armor.
    /// </summary>
    /// <param name="item">item instance to move</param>
    /// <returns>whether the item instance was successfully moved</returns>
    public bool MoveItemInstance(ItemInstance item)
    {
        if (item == null || _currentWeight + (item.item.GetWeight() * item.stack) > _maxWeight || _items.Count >= _maxItems)
            return false;

        _items.Add(item);
        _currentWeight += item.item.GetWeight() * item.stack;

        if (onItemAdded != null)
            onItemAdded();

        return true;
    }

    /// <summary>
    /// Adds the item to the container
    /// </summary>
    /// <param name="item">the item to add</param>
    /// <param name="amount">the count of the item to add</param>
    /// <returns>the amount of items not able to be added</returns>
    public int AddItem(ItemInstance item, int amount = 1)
    {
        //check for space in container
        if (item == null || _currentWeight + item.item.GetWeight() > _maxWeight || _items.Count >= _maxItems)
            return amount;

        //limit items put in container by weight and space
        int weightLimit = (int)((_maxWeight - _currentWeight) / item.item.GetWeight());
        int originalLimit = Mathf.Min(amount, weightLimit);
        int limit = originalLimit;

        //fill stackable slots
        for (int i = 0; i < _items.Count && limit > 0; i++)
        {
            //check for space in stack
            if (item.item == _items[i].item && _items[i].stack < item.item.GetMaxStackSize())
            {
                //stack
                int space = item.item.GetMaxStackSize() - _items[i].stack;
                _items[i].stack += limit;
                if (_items[i].stack > item.item.GetMaxStackSize())
                {
                    _items[i].stack = item.item.GetMaxStackSize();
                }
                limit -= space;
            }
        }

        if (limit > 0 && _items.Count < _maxItems)
        {
            //create new slots
            for (int i = _items.Count; i < _maxItems && limit > 0; i++)
            {
                //check if this is a non stackable item
                if (item.item.GetMaxStackSize() == 1)
                {
                    //keep correct instance properties
                    ItemInstance instance;
                    if (item is GunItemInstance)
                    {
                        GunItemInstance gii = (GunItemInstance)item;
                        instance = gii.gun.MakeItemInstance();
                        (instance as GunItemInstance).mag = gii.mag;
                        (instance as GunItemInstance).condition = gii.condition;
                    }
                    else if (item is ArmorItemInstance)
                    {
                        ArmorItemInstance aii = (ArmorItemInstance)item;
                        instance = aii.armor.MakeItemInstance();
                        (instance as ArmorItemInstance).condition = aii.condition;
                    }
                    else
                    {
                        instance = item.item.MakeItemInstance();
                    }
                    _items.Add(instance);
                    limit--;
                }
                else
                {
                    //create stack
                    ItemInstance stack = item.item.MakeItemInstance();

                    //set the right count on the stack
                    stack.stack = limit;
                    if (stack.stack > item.item.GetMaxStackSize())
                    {
                        stack.stack = item.item.GetMaxStackSize();
                    }
                    limit -= stack.stack;

                    //add to items
                    _items.Add(stack);
                }
            }
        }

        //update weight
        int addedCount = originalLimit - Mathf.Max(limit, 0);
        _currentWeight += addedCount * item.item.GetWeight();

        //trigger event
        if (onItemAdded != null)
            onItemAdded();

        return amount - addedCount;
    }

    /// <summary>
    /// Removes the item at the given index
    /// </summary>
    /// <param name="index">index to remove item from</param>
    /// <param name="amount">amount of items to remove</param>
    /// <returns>how many items weren't able to be removed</returns>
    public int RemoveItemAtIndex(int index, int amount = 1)
    {
        //check that the item exists
        if (index >= _items.Count)
            return amount;

        //remove item from container
        int remainder = amount - _items[index].stack;
        _items[index].stack -= amount;
        _currentWeight -= _items[index].item.GetWeight() * (amount);
        if (_items[index].stack <= 0)
        {
            _currentWeight += _items[index].item.GetWeight() * (-_items[index].stack);
            _items.RemoveAt(index);
        }

        //trigger event
        if (onItemRemoved != null)
            onItemRemoved();

        return Mathf.Max(0, remainder);
    }

    /// <summary>
    /// Removes the last occurances of the item
    /// </summary>
    /// <param name="item">item to remove</param>
    /// <returns>how many items weren't able to be removed</returns>
    public int RemoveItem(Item item, int amount = 1)
    {
        int left = amount;
        for (int i = _items.Count - 1; i >= 0 && left > 0; i--)
        {
            if (item == _items[i].item)
            {
                left = RemoveItemAtIndex(i, left);
            }
        }
        return left;
    }

    public ItemInstance GetItemAtIndex(int index) { return index < _items.Count ? _items[index] : null; }

    public int GetIndexOfItemInstance(ItemInstance instance)
    {
        for (int i = 0; i < _items.Count; i++)
            if (_items[i] == instance)
                return i;
        return -1;
    }

    public bool ContainsItem(Item item)
    {
        for (int i = 0; i < _items.Count; i++)
            if (_items[i].item == item)
                return true;
        return false;
    }

    public List<ItemInstance> GetItemsList() { return _items; }

    public float GetWeight() { return _currentWeight; }
    public float GetMaxWeight() { return _maxWeight; }
    public int GetItemCount() { return _items.Count; }
    public int GetMaxItems() { return _maxItems; }

    //Add and Remove events
    public void RegisterItemAddEvent(Action action) { onItemAdded += action; }
    public void RegisterItemRemovedEvent(Action action) { onItemRemoved += action; }
    public void UnregisterItemAddEvent(Action action) { onItemAdded -= action; }
    public void UnregisterItemRemovedEvent(Action action) { onItemRemoved -= action; }

    public List<ItemInstance> GetItemsForSlot(EGearSlot gearSlot)
    {
        List<ItemInstance> validItems = new List<ItemInstance>();
        for (int i = 0; i < _items.Count; i++)
        {
            if ((gearSlot == EGearSlot.Helmet && _items[i] is ArmorItemInstance && ((_items[i] as ArmorItemInstance).armor.GetArmorType() == EArmorType.Helmet)) ||
                (gearSlot == EGearSlot.Body_Armor && _items[i] is ArmorItemInstance && ((_items[i] as ArmorItemInstance).armor.GetArmorType() == EArmorType.Body_Armor)) ||
                ((gearSlot == EGearSlot.Primary_Weapon || gearSlot == EGearSlot.Secondary_Weapon) && _items[i] is GunItemInstance) ||
                ((gearSlot == EGearSlot.Left_Gadget || gearSlot == EGearSlot.Right_Gadget) && _items[i] is GadgetItemInstance))
            {
                validItems.Add(_items[i]);
            }
        }
        return validItems;
    }
}
