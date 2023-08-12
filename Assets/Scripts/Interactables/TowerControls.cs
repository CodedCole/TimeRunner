using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerControls : MonoBehaviour, IInteractable
{
    [SerializeField] private float _interactionDuration = 0.5f;

    private bool _interacting = false;
    private float _interactionStartTime;

    private RaidManager _raidManager;
    private bool _moving = false;
    private int _nextLevel = 1;
    private GameObject _actor;
    private bool _hasKey = false;
    private Inventory _playerInventory;

    private void Awake()
    {
        _raidManager = FindObjectOfType<RaidManager>();
        _raidManager.onLevelEntered += (x) => _moving = false;
        _playerInventory = FindObjectOfType<Player>().GetComponent<Inventory>();
        _playerInventory.RegisterOnStart(() =>
        {
            _playerInventory.GetContainer().RegisterItemAddEvent(UpdateHaveKey);
            _playerInventory.GetContainer().RegisterItemRemovedEvent(UpdateHaveKey);
        });
    }

    void Update()
    {
        if (!_moving && _interacting && Time.time - _interactionStartTime > _interactionDuration)
        {
            _moving = true;

            bool moved = _raidManager.MoveToLevel(_nextLevel);
            if (!moved)
                _moving = false;
            _nextLevel = Mathf.Abs(_nextLevel - 1);
            EndInteract(_actor);
        }
    }

    void UpdateHaveKey()
    {
        if (_raidManager == null)
            return;

        Item key = _raidManager.GetKeyToNextLevel();
        if (key == null)
            return;

        _hasKey = _playerInventory.GetContainer().ContainsItem(_raidManager.GetKeyToNextLevel());
    }

    public void StartInteract(GameObject actor)
    {
        if (!_hasKey)
            return;

        _interacting = true;
        _interactionStartTime = Time.time;
        _actor = actor;
        enabled = true;
    }

    public void EndInteract(GameObject actor)
    {
        _interacting = false;
        _actor = null;
        enabled = false;
    }

    public string GetInteractDescription()
    {
        return _hasKey ? (_interacting ? "Moving Up..." : "Move Up") : "Requires " + _raidManager.GetKeyToNextLevel()?.GetItemName();
    }

    public float GetInteractProgress()
    {
        return _interacting ? (Time.time - _interactionStartTime) / _interactionDuration : 0.0f;
    }

    public bool IsInteractable()
    {
        return !_moving;
    }
}
