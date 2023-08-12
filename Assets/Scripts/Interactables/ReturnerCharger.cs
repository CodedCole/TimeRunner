using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnerCharger : MonoBehaviour, IInteractable
{
    [SerializeField] private float _chargeDuration = 20.0f;
    [SerializeField] private float _startChargeInteractionTime = 2.0f;
    [Header("Audio")]
    [SerializeField] private AnimationCurve _chargeVolume;
    [SerializeField] private AnimationCurve _chargePitch;
    [SerializeField] private float _fadeOutDuration;
    [SerializeField] private AnimationCurve _fadeOutVolume;
    [SerializeField] private AnimationCurve _fadeOutPitch;

    private bool _interacting = false;
    private float _startInteractTime;
    private bool _charging = false;
    private float _startChargingTime;

    private AudioSource _audio;
    private float c_inverseChargeDuration;
    private GameObject _actor;

    void Awake()
    {
        enabled = false;
        c_inverseChargeDuration = 1 / _chargeDuration;
        _audio = GetComponentInChildren<AudioSource>();
        _audio.volume = _chargeVolume.Evaluate(0);
        _audio.pitch = _chargePitch.Evaluate(0);

        FindObjectOfType<RaidManager>().RegisterOnRaidEnd(OnEndRaid);
    }

    public void StartInteract(GameObject actor)
    {
        _interacting = true;
        _startInteractTime = Time.time;
        _actor = actor;
        enabled = true;
    }

    public void EndInteract(GameObject actor)
    {
        _actor = null;
        _interacting = false;
        if (!_charging)
            enabled = false;
    }

    public float GetInteractProgress()
    {
        return _interacting ? (Time.time - _startInteractTime) / _startChargeInteractionTime : 0.0f;
    }

    public string GetInteractDescription()
    {
        return _interacting ? "Placing Returner..." : "Place Returner";
    }

    public bool IsInteractable()
    {
        return !_charging;
    }

    void Update()
    {
        if (_charging)
        {
            float currentChargeAmount = (Time.time - _startChargingTime) * c_inverseChargeDuration;
            _audio.volume = _chargeVolume.Evaluate(currentChargeAmount);
            _audio.pitch = _chargePitch.Evaluate(currentChargeAmount);
            if (currentChargeAmount >= 1)
            {
                FindObjectOfType<RaidManager>().EndRaid(true);
            }
        }

        if (_interacting && Time.time - _startInteractTime > _startChargeInteractionTime)
        {
            //interaction complete
            _charging = true;
            EndInteract(_actor);
            _startChargingTime = Time.time;
            _audio.Play();
        }
    }

    void OnEndRaid()
    {
        StartCoroutine(EndRaidFade());
    }

    IEnumerator EndRaidFade()
    {
        _charging = false;
        float start = Time.time;
        while(Time.time < start + _fadeOutDuration)
        {
            _audio.pitch = _fadeOutPitch.Evaluate((Time.time - start) / _fadeOutDuration);
            _audio.volume = _fadeOutVolume.Evaluate((Time.time - start) / _fadeOutDuration);
            yield return null;
        }
        _audio.Stop();
        enabled = false;
    }
}
