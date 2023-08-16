using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGadgetItem", menuName = "Item/Gadget/Basic")]
public class GadgetItem : Item
{
    [field: SerializeField] public bool ConsumedOnUse { get; private set; }

    public override ItemInstance MakeItemInstance()
    {
        return new GadgetItemInstance(this);
    }
}
