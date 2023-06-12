using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorItemInstance : ItemInstance
{
    public override Item item
    {
        get {
            return armor;
        } 
    }
    private ArmorItem armor;

    public ArmorItemInstance(ArmorItem templateItem) : base(templateItem)
    {
        armor = templateItem;
    }
}
