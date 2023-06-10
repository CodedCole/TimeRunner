using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset _itemListTemplate;
    [SerializeField] private string _hiddenClass = "hidden";
    [SerializeField] private int _slotsPerRow = 4;

    UIDocument _hud;

    ProgressBar _interactBar;

    VisualElement _inventoryPanel;

    InventoryMenu _inventoryMenu;

    VisualElement _reticle;

    VisualElement _debugPanel;
    Label _mousePos;

    ContainerListController _inventoryItemListController;
    ContainerListController _lootItemListController;

    Action onOpenInventory;
    Action onCloseInventory;

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

        _inventoryPanel.RegisterCallback<MouseEnterEvent>(EnterInventoryPanel);
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
        _interactBar.value = interactable.GetInteractCompletion();
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
        _reticle.transform.position = pos;

        //debug text
        _mousePos.text = "mouse-pos: (x: " + pos.x + ", y: " + pos.y + ")";
    }
}
