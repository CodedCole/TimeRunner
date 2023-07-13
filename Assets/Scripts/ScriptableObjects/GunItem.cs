using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "NewGunItem", menuName = "Item/Gun")]
public class GunItem : Item
{
    [SerializeField] private GunStats stats;
    [SerializeField] private Item ammunition;
    [SerializeField] private Sprite sprite;
    [SerializeField] private SpriteLibraryAsset spriteLibrary;
    [SerializeField] private AnimatorController animatorController;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Vector2 projectileSpawn;

    public GunStats GetStats() { return stats; }
    public Item GetAmmo() { return ammunition; }
    public Sprite GetSprite() { return sprite; }
    public SpriteLibraryAsset GetSpriteLibrary() { return spriteLibrary; }
    public AnimatorController GetAnimatorController() { return animatorController; }
    public Projectile GetProjectile() { return projectile; }
    public Vector2 GetProjectileSpawn() { return projectileSpawn; }

    public override ItemInstance MakeItemInstance()
    {
        return new GunItemInstance(this);
    }
}
