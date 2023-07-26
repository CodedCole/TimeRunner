using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingController : MonoBehaviour
{
    [Serializable]
    public struct FlashEffect
    {
        public Volume volume;
        public float fadeIn;
        public float fadeOut;

        public IEnumerator PlayFlashEffect()
        {
            volume.weight = 0;
            bool fadingIn = true;
            while (fadingIn)
            {
                volume.weight += Time.deltaTime / fadeIn;
                if (volume.weight >= 1)
                {
                    volume.weight = 1;
                    fadingIn = false;
                }
                yield return null;
            }

            bool fadingOut = true;
            while (fadingOut)
            {
                volume.weight -= Time.deltaTime / fadeOut;
                if (volume.weight <= 0)
                {
                    volume.weight = 0;
                    fadingOut = false;
                }
                yield return null;
            }
        }
    }

    [Serializable]
    public struct LerpEffect
    {
        public Volume volume;
        public float transitionDuration;
        public float beginThreshold;
        public float endThreshold;

        private float currentValue;
        private float thresholdLength;

        public void Init(float startingValue)
        {
            currentValue = startingValue;
            thresholdLength = endThreshold - beginThreshold;
            volume.weight = Mathf.Clamp01((currentValue - beginThreshold) / thresholdLength);
        }

        public void SetCurrentValue(float v)
        {
            currentValue = v;
        }

        public IEnumerator PlayEffect(float newValue)
        {
            if (newValue > 1 || newValue < 0)
                yield break;

            float distance = (newValue - currentValue) / transitionDuration;

            bool reachedTarget = false;
            while(!reachedTarget)
            {
                SetCurrentValue(Mathf.Clamp01(currentValue + (Time.deltaTime * distance)));
                if ((distance >= 0 && currentValue >= newValue) || (distance < 0 && currentValue <= newValue))
                {
                    reachedTarget = true;
                    currentValue = newValue;
                }

                volume.weight = Mathf.Clamp01((currentValue - beginThreshold) / thresholdLength);
                yield return null;
            }
        }
    }

    [SerializeField] private Volume baseVolume;
    [SerializeField] private FlashEffect _takeDamageEffect;
    [SerializeField] private LerpEffect _lowHealthEffect;

    private Coroutine current_takeDamageEffectRoutine;
    private Coroutine current_lowHealthEffectRoutine;
    private float currentHealthAmount;

    private void Awake()
    {
        Health h = FindObjectOfType<Player>().GetComponentInChildren<Health>();
        h.RegisterOnTakeDamage(TakeDamageEffects);
        _lowHealthEffect.Init(1);
        h.RegisterOnHealthChange(UpdateLowHealth);
    }

    void TakeDamageEffects() 
    { 
        if (current_takeDamageEffectRoutine != null)
            StopCoroutine(current_takeDamageEffectRoutine);
        current_takeDamageEffectRoutine = StartCoroutine(_takeDamageEffect.PlayFlashEffect());
    }

    void UpdateLowHealth(float currentPercent) 
    {
        _lowHealthEffect.SetCurrentValue(currentHealthAmount);
        currentHealthAmount = currentPercent;
        if(current_lowHealthEffectRoutine != null)
            StopCoroutine(current_lowHealthEffectRoutine);
        current_lowHealthEffectRoutine = StartCoroutine(_lowHealthEffect.PlayEffect(currentPercent));
    }
}
