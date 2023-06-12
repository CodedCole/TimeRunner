using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    [SerializeField] private float _unlockTime = 1.0f;
    [SerializeField] private Sprite _openedSprite;
    [SerializeField] private float _maxWeight;
    [SerializeField] private int _maxItems;
    [SerializeField] private List<Item> _startingItems;

    private Container _container;

    private bool _opened;
    private bool _interacting;
    private float _interactionStartTime;

    void Start()
    {
        _container = new Container(_maxWeight, _maxItems);
        foreach(var i in _startingItems)
        {
            _container.AddItem(i.MakeItemInstance());
        }
    }

    void Update()
    {
        if (_interacting)
        {
            if (Time.time - _interactionStartTime >= _unlockTime)
            {
                _opened = true;
                GetComponent<SpriteRenderer>().sprite = _openedSprite;
                _interacting = false;

                FindObjectOfType<HUD>().ShowInventoryList(_container);
            }
        }
    }

    public void StartInteract()
    {
        _interacting = true;
        _interactionStartTime = Time.time;
        enabled = true;
    }

    public void EndInteract()
    {
        _interacting = false;
        enabled = false;
    }

    public float GetInteractCompletion()
    {
        return _interacting ? (Time.time - _interactionStartTime) / _unlockTime : 0;
    }

    public bool IsInteractable()
    {
        return true;
    }

    public string GetInteractDescription()
    {
        return _interacting ? "Opening..." : "Open";
    }
}
