using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHealthGadgetItem", menuName = "Item/Gadget/Health Gadget")]
public class HealthGadgetItem : GadgetItem
{
    [field: SerializeField] public float HealthRestored { get; private set; }
    [field: SerializeField] public float UseDuration { get; private set; }

    public override ItemInstance MakeItemInstance()
    {
        return new HealthGadgetItemInstance(this);
    }
}
