using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class Player : MonoBehaviour, Controls.IDuringRunActions
{
    private TopDownMovement _movement;
    private Inventory _inventory;
    private GunController _gunController;
    private HUD _hud;
    private Camera _camera;
    private Controls _controls;

    private List<IInteractable> _interactables = new List<IInteractable>();
    private bool _inventoryOpened = false;
    private bool _firing = false;

    // Start is called before the first frame update
    void Start()
    {
        _movement = GetComponent<TopDownMovement>();
        _inventory = GetComponent<Inventory>();
        _gunController = GetComponent<GunController>();
        _hud = FindObjectOfType<HUD>();
        _hud.RegisterOnOpenInventory(() => { _inventoryOpened = true; });
        _hud.RegisterOnCloseInventory(() => { _inventoryOpened = false; });
        _camera = FindObjectOfType<Camera>();
        _camera.GetComponent<CameraFollow>().SetTarget(transform);
    }

    private void OnEnable()
    {
        if (_controls == null)
        {
            _controls = new Controls();
            _controls.DuringRun.SetCallbacks(this);
        }
        _controls.DuringRun.Enable();
    }

    private void OnDisable()
    {
        _controls.DuringRun.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (_interactables.Count > 0 && _interactables[0].IsInteractable())
        {
            _hud.ShowInteractBar();
            _hud.SetInteractBarValue(_interactables[0]);
        }
        else
            _hud.HideInteractBar();

        if (_firing)
        {
            _gunController.Fire();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable i = collision.GetComponentInChildren<IInteractable>();
        if (i != null)
        {
            _interactables.Add(i);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable i = collision.GetComponentInChildren<IInteractable>();
        if (i != null)
        {
            _interactables.Remove(i);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movement.UpdateMove(context.ReadValue<Vector2>());
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (_interactables.Count == 0)
            return;

        // interact start
        if (context.started && _interactables[0].IsInteractable())
        {
            Debug.Log("Started");
            _interactables[0].StartInteract();
        }
        // interact finish
        if (context.performed)
        {
            Debug.Log("Interact");
            _interactables[0].EndInteract();
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (context.control.magnitude > 0.5f && !_firing)
            {
                _gunController.Fire();
                _firing = true;
                Debug.Log("Firing");
            }
            else if (context.control.magnitude < 0.5f && _firing)
            {
                _firing = false;
                Debug.Log("Stopped Firing");
            }
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_inventoryOpened)
            {
                _hud.ShowInventoryList(null);
            }
            else
            {
                _hud.HideInventoryList();
            }
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (context.control.name == "rightStick")
            {
                Vector2 v = context.ReadValue<Vector2>();
                Vector2 retPos = _camera.WorldToScreenPoint(((Vector2)transform.position) + v);
                _hud.MoveReticle(Vector2.down * retPos.y + Vector2.right * retPos.x);
                _gunController.AimAtPos(((Vector2)transform.position) + v);
            }
            else
            {
                Vector2 v = context.ReadValue<Vector2>();
                _hud.MoveReticle(Vector2.down * v.y + Vector2.right * v.x);
                _gunController.AimAtPos(_camera.ScreenToWorldPoint(v));
            }
        }
    }
}
