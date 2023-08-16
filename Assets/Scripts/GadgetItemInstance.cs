using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetItemInstance : ItemInstance
{
    public override Item item { get { return gadget; } }
    public GadgetItem gadget { get; private set; }

    public event Action<GadgetItemInstance> OnGadgetConsumed;
    public GadgetItemInstance(GadgetItem templateItem) : base(templateItem)
    {
        gadget = templateItem;
    }

    public virtual void Use(GameObject user)
    {
        if (gadget.ConsumedOnUse)
        {
            this.stack--;
            OnGadgetConsumed(this);
        }
    }
}
