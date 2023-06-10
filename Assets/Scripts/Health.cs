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

    private float _damageFlashTimer;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        _health -= amount;
        _damageFlashTimer = _damageFlashDuration;

        if (_health <= 0)
        {
            _spriteRenderer.color = _deathColor;
            enabled = false;
            return true;
        }
        return false;
    }
}
