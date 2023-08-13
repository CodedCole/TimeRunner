using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetItemInstance : ItemInstance
{
    public override Item item { get { return gadget; } }
    public GadgetItem gadget { get; private set; }

    public GadgetItemInstance(GadgetItem templateItem) : base(templateItem)
    {

    }

    public virtual void Use()
    {

    }
}
