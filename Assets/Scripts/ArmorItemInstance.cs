using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorItemInstance : ItemInstance
{
    public override Item item { get { return armor; } }
    public ArmorItem armor { get; private set; }

    public float condition;

    public ArmorItemInstance(ArmorItem templateItem) : base(templateItem)
    {
        armor = templateItem;
        condition = 100.0f;
    }
}
