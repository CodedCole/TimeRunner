using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGunItem", menuName = "Item/Gun")]
public class GunItem : Item
{
    [SerializeField] private GunStats stats;

    public GunStats GetStats() { return stats; }
}
