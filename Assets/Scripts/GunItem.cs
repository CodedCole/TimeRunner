using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGunItem", menuName = "Item/Gun")]
public class GunItem : Item
{
    [SerializeField] private GunStats stats;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Vector2 projectileSpawn;

    public GunStats GetStats() { return stats; }
    public Projectile GetProjectile() { return projectile; }
    public Vector2 GetProjectileSpawn() { return projectileSpawn; }
}
