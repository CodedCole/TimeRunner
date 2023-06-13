using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCan : MonoBehaviour, IInteractable
{
    [SerializeField] private float discardTime = 2;

    private bool _discarding;
    private float _discardStartTime;

    public void StartInteract()
    {
        _discarding = true;
        _discardStartTime = Time.time;
        enabled = true;
    }

    public void EndInteract()
    {
        _discarding = false;
        enabled = false;
    }

    public float GetInteractCompletion()
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
            EndInteract();
        }
    }
}
