using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCan : MonoBehaviour, IInteractable
{
    [SerializeField] private float discardTime = 2;

    private bool _discarding;
    private float _discardStartTime;
    private GameObject _actor;

    public void StartInteract(GameObject actor)
    {
        _discarding = true;
        _discardStartTime = Time.time;
        _actor = actor;
        enabled = true;
    }

    public void EndInteract(GameObject actor)
    {
        _discarding = false;
        _actor = null;
        enabled = false;
    }

    public float GetInteractProgress()
    {
        return _discarding ? (Time.time - _discardStartTime) / discardTime : 0.0f;
    }

    public string GetInteractDescription()
    {
        return _discarding ? "Discarding..." : "Discard";
    }

    public bool IsInteractable()
    {
        return true;
    }

    void Update()
    {
        if (_discarding && Time.time - _discardStartTime >= discardTime)
        {
            FindObjectOfType<Inventory>().GetContainer().RemoveItemAtIndex(0, 1);
            EndInteract(_actor);
        }
    }
}
