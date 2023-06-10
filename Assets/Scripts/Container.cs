using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    public class ItemStack
    {
        public Item item { get; protected set; }
        public int count { get; protected set; }
    }

    private float _maxWeight;
    private int _maxItems;
    
    private List<Item> _items = new List<Item>();
    private float _currentWeight = 0;

    Action onItemAdded;
    Action onItemRemoved;

    public Container(float maxWeight, int maxItems) 
    { 
        _maxWeight = maxWeight;
        _maxItems = maxItems;
    }

    // returns true if the item was able to fit in the container
    public bool AddItem(Item item)
    {
        //check for space in container
        if (item == null || _currentWeight + item.GetWeight() > _maxWeight || _items.Count >= _maxItems)
            return false;

        //add item into container
        _items.Add(item);
        _currentWeight += item.GetWeight();

        //trigger event
        if (onItemAdded != null)
            onItemAdded();

        return true;
    }

    public Item GetItemAtIndex(int index) { return index < _items.Count ? _items[index] : null; }

    /// <summary>
    /// Removes the item at the given index
    /// </summary>
    /// <param name="index">index to remove item from</param>
    /// <returns>whether the remove was successful</returns>
    public bool RemoveItemAtIndex(int index)
    {
        //check that the item exists
        if (index >= _items.Count)
            return false;

        //remove item from container
        _currentWeight -= _items[index].GetWeight();
        _items.RemoveAt(index);

        //trigger event
        if (onItemRemoved != null)
            onItemRemoved();

        return true;
    }

    /// <summary>
    /// Removes the last occurance of the item
    /// </summary>
    /// <param name="item">item to remove</param>
    /// <returns>whether the item was successfully removed</returns>
    public bool RemoveItem(Item item)
    {
        for (int i = _items.Count; i >= 0; i--)
        {
            if (item == _items[i])
            {
                RemoveItemAtIndex(i);
                return true;
            }
        }
        return false;
    }

    public List<Item> GetItemsList() { return _items; }

    public float GetWeight() { return _currentWeight; }
    public float GetMaxWeight() { return _maxWeight; }
    public int GetItemCount() { return _items.Count; }
    public int GetMaxItems() { return _maxItems; }

    //Add and Remove events
    public void RegisterItemAddEvent(Action action) { onItemAdded += action; }
    public void RegisterItemRemovedEvent(Action action) { onItemRemoved += action; }
    public void UnregisterItemAddEvent(Action action) { onItemAdded -= action; }
    public void UnregisterItemRemovedEvent(Action action) { onItemRemoved -= action; }

    public List<Item> GetItemsForSlot(EGearSlot gearSlot)
    {
        List<Item> validItems = new List<Item>();
        for (int i = 0; i < _items.Count; i++)
        {
            if ((gearSlot == EGearSlot.Helmet && _items[i] is ArmorItem && (_items[i] as ArmorItem).GetArmorType() == EArmorType.Helmet) ||
                (gearSlot == EGearSlot.Body_Armor && _items[i] is ArmorItem && (_items[i] as ArmorItem).GetArmorType() == EArmorType.Body_Armor) ||
                ((gearSlot == EGearSlot.Primary_Weapon || gearSlot == EGearSlot.Secondary_Weapon) && _items[i] is GunItem) ||
                ((gearSlot == EGearSlot.Left_Gadget || gearSlot == EGearSlot.Right_Gadget) && !(_items[i] is GunItem) && !(_items[i] is ArmorItem)))
            {
                validItems.Add(_items[i]);
            }
        }
        return validItems;
    }
}
