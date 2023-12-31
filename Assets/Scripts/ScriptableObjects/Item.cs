using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "Item/Generic Item")]
public class Item : ScriptableObject
{
    [SerializeField] private string itemName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private float weight;
    [Min(1)]
    [SerializeField] private int maxStackSize;

    public string GetItemName() { return itemName; }
    public string GetDescription() { return description; }
    public Sprite GetIcon() { return icon; }
    public float GetWeight() { return weight; }
    public int GetMaxStackSize() { return maxStackSize; }

    public virtual ItemInstance MakeItemInstance()
    {
        return new ItemInstance(this);
    }
}
