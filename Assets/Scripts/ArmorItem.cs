using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArmorItem", menuName = "Item/Armor")]
public class ArmorItem : Item
{
    [SerializeField] private EArmorType armorType;
    [SerializeField] private ArmorStats stats;

    public EArmorType GetArmorType() { return armorType; }
    public ArmorStats GetStats() { return stats; }

    public override ItemInstance MakeItemInstance()
    {
        return new ArmorItemInstance(this);
    }
}
