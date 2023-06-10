using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDamageType { Kinetic, Heat, Electric, Gravity };

public enum EArmorType { Helmet, Body_Armor };

public enum EGearSlot { Helmet, Body_Armor, Primary_Weapon, Secondary_Weapon, Left_Gadget, Right_Gadget }

[Serializable]
public struct ArmorStats
{
    public float kineticResistance;
    public float heatResistance;
    public float electricResistance;
    public float gravityResistance;
}

[Serializable]
public struct GunStats
{
    public float damage;
    public EDamageType damageType;
    public float rateOfFire;
    public int magSize;
    public float reloadTime;
}
