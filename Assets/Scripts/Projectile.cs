using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime;
    public float damage;
    public EDamageType damageType;
    public GameObject hitEffect;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Health hit = collision.collider.GetComponentInParent<Health>();
        if (hit != null)
        {
            hit.TakeDamage(damage, damageType);
        }

        if(hitEffect != null)
            Instantiate(hitEffect, collision.GetContact(0).point, Quaternion.identity);

        Destroy(gameObject);
    }
}
