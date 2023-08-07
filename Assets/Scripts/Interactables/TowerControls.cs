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

    public void StartInteract()
    {
        _interacting = true;
        enabled = true;
    }

    public void EndInteract()
    {
        _interacting = false;
        enabled = false;
    }

    public string GetInteractDescription()
    {
        return _interacting ? "Moving Up..." : "Move Up";
    }

    public float GetInteractProgress()
    {
        return _interacting ? (Time.time - _interactionStartTime) / _interactionDuration : 0.0f;
    }

    public bool IsInteractable()
    {
        return !_moving;
    }

    void Awake()
    {
        _raidManager = FindObjectOfType<RaidManager>();
        _raidManager.onLevelEntered += (x) => _moving = false;
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
            EndInteract();
        }
    }
}
