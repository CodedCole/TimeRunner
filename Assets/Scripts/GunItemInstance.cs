using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItemInstance : ItemInstance
{
    public override Item item { get { return gun; } }
    private GunItem gun;

    public GunItemInstance(GunItem templateItem) : base(templateItem)
    {
        gun = templateItem;
    }
}
