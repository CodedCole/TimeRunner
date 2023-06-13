using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItemInstance : ItemInstance
{
    public override Item item { get { return gun; } }
    public GunItem gun { get; private set; }

    public int mag;
    public float condition;

    public GunItemInstance(GunItem templateItem) : base(templateItem)
    {
        gun = templateItem;
        mag = templateItem.GetStats().magSize;
        condition = 100.0f;
    }
}
