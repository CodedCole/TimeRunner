using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnimationEventDistributer : MonoBehaviour
{
    public CustomSpriteResolver _spriteResolver;
    private string _label;

    public string Label 
    { 
        get { return _label; }
        set 
        { 
            _label = value;
            UpdateSpriteResolver();
        }
    }

    void UpdateSpriteResolver()
    {
        if (_spriteResolver != null)
            _spriteResolver.Label = _label;
        else
            Debug.LogWarning("No CustomSpriteResolver");
    }
}
