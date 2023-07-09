using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private ArmorStats _armorStats;

    [Header("Damage Flash")]
    [SerializeField] private Color _damageFlashColor;
    [SerializeField] private float _damageFlashDuration;

    [Header("Death")]
    [SerializeField] private Color _deathColor;
    
    private float _health;
    private SpriteRenderer _spriteRenderer;
    private Inventory _inventory;

    private float _damageFlashTimer;

    private Action onDeath;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _inventory = GetComponent<Inventory>();
        if (_inventory != null)
            _inventory.RegisterOnEquip(UpdateArmor);
        _health = _maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (_damageFlashTimer > 0)
        {
            _damageFlashTimer -= Time.deltaTime;
            _spriteRenderer.color = _damageFlashColor;
        }
        else
        {
            _spriteRenderer.color = Color.white;
        }
    }

    // returns true if the damage is fatal
    public bool TakeDamage(float amount, EDamageType damageType)
    {
        if (_health <= 0)
            return false;

        switch(damageType)
        {
            case EDamageType.Kinetic:
                _health -= amount * (1.0f - _armorStats.kineticResistance);
                break;
            case EDamageType.Heat:
                _health -= amount * (1.0f - _armorStats.heatResistance);
                break;
            case EDamageType.Electric:
                _health -= amount * (1.0f - _armorStats.electricResistance);
                break;
            case EDamageType.Gravity:
                _health -= amount * (1.0f - _armorStats.gravityResistance);
                break;
        }
        _damageFlashTimer = _damageFlashDuration;

        if (_health <= 0)
        {
            _spriteRenderer.color = _deathColor;
            enabled = false;

            if (onDeath != null)
                onDeath();

            return true;
        }
        return false;
    }

    public void RegisterOnDeath(Action action) { onDeath += action; }
    public void UnregisterOnDeath(Action action) { onDeath -= action; }

    void UpdateArmor()
    {
        _armorStats = _inventory.GetCurrentArmorStats();
    }
}
