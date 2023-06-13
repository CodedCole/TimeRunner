using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    public class ItemStack
    {
        public ItemInstance itemInstance { get; set; }
        public int count { get; set; }
    }

    private float _maxWeight;
    private int _maxItems;
    
    private List<ItemStack> _items = new List<ItemStack>();
    private float _currentWeight = 0;

    Action onItemAdded;
    Action onItemRemoved;

    public Container(float maxWeight, int maxItems) 
    { 
        _maxWeight = maxWeight;
        _maxItems = maxItems;
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
            if (item.item == _items[i].itemInstance.item && _items[i].count < item.item.GetMaxStackSize())
            {
                int space = item.item.GetMaxStackSize() - _items[i].count;
                _items[i].count += limit;
                if (_items[i].count > item.item.GetMaxStackSize())
                {
                    _items[i].count = item.item.GetMaxStackSize();
                }
                limit -= space;
            }
        }

        if (limit > 0 && _items.Count < _maxItems)
        {
            //create new slots
            for (int i = _items.Count; i < _maxItems && limit > 0; i++)
            {
                //create stack
                ItemStack stack = new ItemStack();
                stack.itemInstance = item;

                //set the right count on the stack
                stack.count = limit;
                if (stack.count > item.item.GetMaxStackSize())
                {
                    stack.count = item.item.GetMaxStackSize();
                }
                limit -= stack.count;

                //add to items
                _items.Add(stack);
            }
        }

        if (limit < 0)
            limit = 0;

        //add item into container
        /*
        ItemStack itemStack = new ItemStack();
        itemStack.itemInstance = item;
        itemStack.count = 1;
        _items.Add(itemStack);
        _currentWeight += item.item.GetWeight();
        /**/
        int addedCount = originalLimit - limit;
        _currentWeight += (originalLimit - limit) * item.item.GetWeight();

        //trigger event
        if (onItemAdded != null)
            onItemAdded();

        return amount - addedCount;
    }

    public ItemStack GetItemAtIndex(int index) { return index < _items.Count ? _items[index] : null; }

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
        int remainder = amount - _items[index].count;
        _items[index].count -= amount;
        _currentWeight -= _items[index].itemInstance.item.GetWeight() * (amount);
        if (_items[index].count <= 0)
        {
            _currentWeight += _items[index].itemInstance.item.GetWeight() * (-_items[index].count);
            _items.RemoveAt(index);
        }

        //trigger event
        if (onItemRemoved != null)
            onItemRemoved();

        return Mathf.Max(0, remainder);
    }

    /// <summary>
    /// Removes the last occurance of the item
    /// </summary>
    /// <param name="item">item to remove</param>
    /// <returns>whether the item was successfully removed</returns>
    public bool RemoveItem(ItemInstance item)
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            if (item == _items[i].itemInstance)
            {
                RemoveItemAtIndex(i);
                return true;
            }
        }
        return false;
    }

    public List<ItemStack> GetItemsList() { return _items; }

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
            if ((gearSlot == EGearSlot.Helmet && _items[i].itemInstance is ArmorItemInstance && ((_items[i].itemInstance as ArmorItemInstance).item as ArmorItem).GetArmorType() == EArmorType.Helmet) ||
                (gearSlot == EGearSlot.Body_Armor && _items[i].itemInstance is ArmorItemInstance && ((_items[i].itemInstance as ArmorItemInstance).item as ArmorItem).GetArmorType() == EArmorType.Body_Armor) ||
                ((gearSlot == EGearSlot.Primary_Weapon || gearSlot == EGearSlot.Secondary_Weapon) && _items[i].itemInstance is GunItemInstance) ||
                ((gearSlot == EGearSlot.Left_Gadget || gearSlot == EGearSlot.Right_Gadget) && !(_items[i].itemInstance is GunItemInstance) && !(_items[i].itemInstance is ArmorItemInstance)))
            {
                validItems.Add(_items[i].itemInstance);
            }
        }
        return validItems;
    }
}
