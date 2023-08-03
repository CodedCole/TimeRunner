using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] private string _hiddenClass = "hidden";

    UIDocument _hud;

    ProgressBar _interactBar;

    VisualElement _inventoryPanel;

    InventoryMenu _inventoryMenu;

    VisualElement _reticle;

    VisualElement _debugPanel;
    Label _mousePos;
    Label _playerCellPos;
    Label _tileData;

    Action onOpenInventory;
    Action onCloseInventory;

    Tilemap _tilemap;
    ZoneGenerator _zoneGenerator;

    // Start is called before the first frame update
    void Start()
    {
        _hud = GetComponent<UIDocument>();
        _inventoryMenu = GetComponent<InventoryMenu>();

        _interactBar = _hud.rootVisualElement.Q<ProgressBar>("InteractBar");

        _inventoryPanel = _hud.rootVisualElement.Q<VisualElement>("Inventory");

        _reticle = _hud.rootVisualElement.Q<VisualElement>("Reticle");

        _debugPanel = _hud.rootVisualElement.Q<VisualElement>("DebugPanel");
        _mousePos = _debugPanel.Q<Label>("mouse-position");
        _playerCellPos = _debugPanel.Q<Label>("player-cell-position");
        _tileData = _debugPanel.Q<Label>("tile-data");

        _inventoryPanel.RegisterCallback<MouseEnterEvent>(EnterInventoryPanel);

        _tilemap = FindObjectOfType<Tilemap>();
        _zoneGenerator = FindObjectOfType<ZoneGenerator>();
    }

    void EnterInventoryPanel(MouseEnterEvent ev)
    {
        Debug.Log("Mouse entered the inventory panel");
    }

    public void ShowInteractBar()
    {
        _interactBar.visible = true;
    }

    public void HideInteractBar()
    {
        _interactBar.visible = false;
    }

    public void SetInteractBarValue(IInteractable interactable)
    {
        _interactBar.value = interactable.GetInteractProgress();
        _interactBar.title = interactable.GetInteractDescription();
    }


    public void RegisterOnOpenInventory(Action action) { onOpenInventory += action; }
    public void UnRegisterOnOpenInventory(Action action) { onOpenInventory -= action; }

    public void RegisterOnCloseInventory(Action action) { onCloseInventory += action; }
    public void UnRegisterOnCloseInventory(Action action) { onCloseInventory -= action; }

    public void ShowInventoryList(Container container)
    {
        //reveal inventory panel
        _inventoryPanel.RemoveFromClassList(_hiddenClass);

        _inventoryMenu.OpenWithLootContainer(container);

        Debug.Log("Open Inventory Event");
        Invoke("OnOpenInventory", 0.1f);
    }

    public void HideInventoryList()
    { 
        _inventoryPanel.AddToClassList(_hiddenClass);

        Debug.Log("Close Inventory Event");
        onCloseInventory();
    }

    void OnOpenInventory()
    {
        onOpenInventory();
    }

    public void MoveReticle(Vector2 pos)
    {   
        //move reticle
        _reticle.transform.position = RuntimePanelUtils.ScreenToPanel(_hud.rootVisualElement.panel, pos); ;

        //debug text
        _mousePos.text = "mouse-pos: (x: " + pos.x + ", y: " + pos.y + ")";
    }

    public void PlayerCellPosition(Vector3 position)
    {
        Vector3Int pos = _tilemap.WorldToCell(new Vector3(position.x, position.y));
        //pos.y += pos.z;
        //pos.x += pos.z;
        _playerCellPos.text = "player-cell: (x: " + pos.x + ", y: " + pos.y + ", z: " + pos.z + ")";
        bool onTile = _tilemap.HasTile(pos);
        _tileData.text = "on-tile: " + onTile.ToString();
        Zone zone = _zoneGenerator.GetZoneAtTile(pos);
        if (zone != null)
            _tileData.text += "\nZone:\n" + zone.data.name;
    }
}
