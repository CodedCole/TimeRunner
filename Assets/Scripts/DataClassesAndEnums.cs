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
    public float projectileVelocity;
}

[Serializable]
struct ItemInitialState
{
    public Item item;
    public int stackSize;
}

public enum EDirection { North, East, South, West }

public static class DirectionHelper
{
    public static EDirection GetOppositeDirection(this EDirection dir)
    {
        return (EDirection)(((int)dir + 2) % 4);
    }

    public static Vector2Int GetDirectionVector(this EDirection dir)
    {
        switch(dir)
        {
            case EDirection.North:
                return new Vector2Int(0, 1);
            case EDirection.East:
                return new Vector2Int(1, 0);
            case EDirection.South:
                return new Vector2Int(0, -1);
            case EDirection.West:
                return new Vector2Int(-1, 0);
            default:
                return new Vector2Int(0, 0);
        }
    }
}
