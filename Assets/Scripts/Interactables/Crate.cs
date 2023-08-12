using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    [SerializeField] private float _unlockTime = 1.0f;
    [SerializeField] private Sprite _openedSprite;
    [SerializeField] private float _maxWeight;
    [SerializeField] private int _maxItems;
    [SerializeField] private string _crateTypeName = "basic_crate";
    [SerializeField] private bool _useStartingItems = false;
    [SerializeField] private List<ItemInitialState> _startingItems;

    private Container _container;

    private bool _interacting;
    private float _interactionStartTime;

    void Awake()
    {
        _container = new Container(_maxWeight, _maxItems);
        ItemInitialState[] loot;
        if (_useStartingItems)
            loot = _startingItems.ToArray();
        else
            loot = LootManager.GetLootForCrateType(_crateTypeName);

        if (loot != null)
        {
            foreach (var i in loot)
            {
                ItemInstance ii = i.item.MakeItemInstance();
                ii.stack = i.stackSize;
                _container.MoveItemInstance(ii);
            }
        }
    }

    void Update()
    {
        if (_interacting)
        {
            if (Time.time - _interactionStartTime >= _unlockTime)
            {
                GetComponent<SpriteRenderer>().sprite = _openedSprite;
                _interacting = false;

                FindObjectOfType<HUD>().ShowInventoryList(_container);
            }
        }
    }

    public void StartInteract(GameObject actor)
    {
        _interacting = true;
        _interactionStartTime = Time.time;
        enabled = true;
    }

    public void EndInteract(GameObject actor)
    {
        _interacting = false;
        enabled = false;
    }

    public float GetInteractProgress()
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

    public string GetCrateTypeName()
    {
        return _crateTypeName;
    }

    public void InsertItem(ItemInitialState itemStack)
    {
        _container.AddItem(itemStack.item.MakeItemInstance(), itemStack.stackSize);
    }
}
