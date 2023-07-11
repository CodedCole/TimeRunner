using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[ExecuteInEditMode]
public class CustomSpriteResolver : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private SpriteLibrary _library;

    private string _category;
    private string _label;

    public string Category 
    { 
        get { return _category; }
        set
        {
            if (_category != value)
            {
                _category = value;
                UpdateSprite();
            }
        }
    }
    public string Label 
    { 
        get { return _label; }
        set 
        { 
            if (_label != value)
            {
                _label = value;
                UpdateSprite();
            }
        }
    }

    public void UpdateSprite()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            throw new System.NullReferenceException("_spriteRenderer is null");
        if (_library == null)
            _library = GetComponentInParent<SpriteLibrary>();
        if (_library == null)
            _library = GetComponent<SpriteLibrary>();
        if (_library == null)
            throw new System.NullReferenceException("_library is null");
        _spriteRenderer.sprite = _library.GetSprite(_category, _label);
    }

    public void SetFlip(bool flip)
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            throw new System.NullReferenceException("_spriteRenderer is null");
        _spriteRenderer.flipX = flip;
    }
}
