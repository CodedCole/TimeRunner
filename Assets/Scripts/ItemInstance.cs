using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstance
{
    public virtual Item item { get; private set; }

    public ItemInstance(Item templateItem)
    {
        item = templateItem;
    }
}
