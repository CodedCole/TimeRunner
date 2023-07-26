using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAnimator : MonoBehaviour
{
    public Material _material;
    public string _property;
    public AnimationCurve _curve;
    public float _duration;

    public bool _animateOnEnable = true;
    public bool _stopOnDisable = true;

    Coroutine _animation;

    private void OnEnable()
    {
        if (_animateOnEnable && _animation == null)
        {
            _animation = StartCoroutine(Animate());
        }
    }

    private void OnDisable()
    {
        if (_stopOnDisable && _animation != null)
        {
            StopCoroutine(_animation);
            _animation = null;
        }
    }

    IEnumerator Animate()
    {
        float startTime = Time.time;
        while(Time.time - startTime < _duration)
        {
            _material.SetFloat(_property, _curve.Evaluate(Time.time - startTime));
            yield return null;
        }
        //End at the end of the curve
        _material.SetFloat(_property, _curve.Evaluate(_duration));
        _animation = null;
    }
}
