using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "NewArmorItem", menuName = "Item/Armor")]
public class ArmorItem : Item
{
    [SerializeField] private EArmorType armorType;
    [SerializeField] private SpriteLibraryAsset spriteLibrary;
    [SerializeField] private ArmorStats stats;

    public EArmorType GetArmorType() { return armorType; }
    public SpriteLibraryAsset GetSpriteLibrary() { return spriteLibrary; }
    public ArmorStats GetStats() { return stats; }

    public override ItemInstance MakeItemInstance()
    {
        return new ArmorItemInstance(this);
    }
}
