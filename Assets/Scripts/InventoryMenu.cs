using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryMenu : MonoBehaviour, Controls.IMenuActions
{
    [SerializeField] private VisualTreeAsset _gearSelectEntryTemplate;
    [SerializeField] private VisualTreeAsset _gearSelectNoItemsFitErrorTemplate;
    [SerializeField] private VisualTreeAsset _itemSlotTemplate;
    [SerializeField] private int _itemSlotsPerRow = 4;
    [SerializeField] private string _selectedClass;
    [SerializeField] private string _hiddenClass = "hidden";

    private Controls _controls;

    private UIDocument _uiDocument;
    private Inventory _inventory;
    private ItemDataCardController _cardController;

                                                //selected slot index for gear
    private VisualElement _helmetSlot;          //-5    top middle
    private VisualElement _bodyArmorSlot;       //-2    bottom middle
    private VisualElement _primarySlot;         //-6    top left
    private VisualElement _secondarySlot;       //-4    top right
    private VisualElement _leftGadgetSlot;      //-3    bottom left
    private VisualElement _rightGadgetSlot;     //-1    bottom right

    private List<ItemInstance> _selectableGear;
    private ScrollView _gearSelectScrollView;
    private int _selectedGearIndex;             //-1 if not selecting gear
    private bool _gearSlotHasItem;

    private int _inventorySize = -1;
    private ScrollView _inventoryScrollView;
    private int _lootContainerSize = -1;
    private ContainerListController _inventoryContainerController;
    private ContainerListController _lootContainerController;
    private VisualElement _lootPanel;

    private int _currentSelectedSlot = -6;      //index of slot (a negative value is one of the gear slots)
    private bool _selectedLoot = false;         // whether the current selection is inside the non-inventory container
    private bool _lootPanelOpen = false;

    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _inventory = FindObjectOfType<Inventory>();
        _inventory.RegisterOnStart(OnInventoryInitialized);
        _inventory.RegisterOnEquip(UpdateGear);

        _helmetSlot = _uiDocument.rootVisualElement.Q<VisualElement>("Helmet");
        _bodyArmorSlot = _uiDocument.rootVisualElement.Q<VisualElement>("BodyArmor");

        _primarySlot = _uiDocument.rootVisualElement.Q<VisualElement>("PrimaryWeapon");
        _secondarySlot = _uiDocument.rootVisualElement.Q<VisualElement>("SecondaryWeapon");

        _leftGadgetSlot = _uiDocument.rootVisualElement.Q<VisualElement>("LeftGadget");
        _rightGadgetSlot = _uiDocument.rootVisualElement.Q<VisualElement>("RightGadget");

        _gearSelectScrollView = _uiDocument.rootVisualElement.Q<ScrollView>("GearSelectScrollView");
        _inventoryScrollView = _uiDocument.rootVisualElement.Q<VisualElement>("InventoryPanel").Q<ScrollView>("ItemSlotScrollView");

        _lootPanel = _uiDocument.rootVisualElement.Q<VisualElement>("LootPanel");

        _cardController = new ItemDataCardController(_uiDocument.rootVisualElement.Q<VisualElement>("ItemDataCard"));

        _selectedGearIndex = -1;        //start by not selecting gear
        _currentSelectedSlot = -6;      //start with primary weapon
    }

    /// <summary>
    /// Responds to the OnStart Inventory event (should never be called unless this event occurs)
    /// </summary>
    void OnInventoryInitialized()
    {
        _inventorySize = _inventory.GetContainer().GetMaxItems();

        _inventoryContainerController = new ContainerListController();
        _inventoryContainerController.InitializeList(_inventoryScrollView, _itemSlotTemplate, ref _inventory.GetContainer(), _itemSlotsPerRow);

        _lootContainerController = new ContainerListController();
        _lootContainerController.InitializeList(
            _lootPanel.Q<ScrollView>("ItemSlotScrollView"),
            _itemSlotTemplate, ref _inventory.GetContainer(), _itemSlotsPerRow);

        HUD hud = FindObjectOfType<HUD>();
        hud.RegisterOnOpenInventory(_inventoryContainerController.UpdateSize);
        hud.RegisterOnOpenInventory(_lootContainerController.UpdateSize);
        hud.RegisterOnOpenInventory(EnableControls);
        hud.RegisterOnCloseInventory(DisableControls);
    }

    void OnEnable()
    {
        //check that inventory was setup
        if (_inventorySize == -1)
            return;

        //register for events to update ui
        _inventory.GetContainer().RegisterItemAddEvent(_inventoryContainerController.UpdateUI);
        _inventory.GetContainer().RegisterItemRemovedEvent(_inventoryContainerController.UpdateUI);
    }

    void OnDisable()
    {
        _inventory.GetContainer().UnregisterItemAddEvent(_inventoryContainerController.UpdateUI);
        _inventory.GetContainer().UnregisterItemRemovedEvent(_inventoryContainerController.UpdateUI);
    }

    /// <summary>
    /// Enables MenuActions callbacks
    /// </summary>
    void EnableControls()
    {
        if (_controls == null)
        {
            _controls = new Controls();
            _controls.Menu.SetCallbacks(this);
        }
        _controls.Menu.Enable();
    }

    /// <summary>
    /// Disables MenuActions callbacks
    /// </summary>
    void DisableControls()
    {
        if (_controls != null)
            _controls.Menu.Disable();
    }
    
    void UpdateGear()
    {
        _helmetSlot.style.backgroundImage = _inventory.helmet != null ? new StyleBackground(_inventory.helmet.item.GetIcon()) : null;
        _bodyArmorSlot.style.backgroundImage = _inventory.bodyArmor!= null ? new StyleBackground(_inventory.bodyArmor.item.GetIcon()) : null;
        _primarySlot.style.backgroundImage = _inventory.primaryWeapon != null ? new StyleBackground(_inventory.primaryWeapon.item.GetIcon()) : null;
        _secondarySlot.style.backgroundImage = _inventory.secondaryWeapon != null ? new StyleBackground(_inventory.secondaryWeapon.item.GetIcon()) : null;
        _leftGadgetSlot.style.backgroundImage = _inventory.leftGadget != null ? new StyleBackground(_inventory.leftGadget.item.GetIcon()) : null;
        _rightGadgetSlot.style.backgroundImage = _inventory.rightGadget != null ? new StyleBackground(_inventory.rightGadget.item.GetIcon()) : null;
    }

    void UpdateItemDataCard() { _cardController.UpdateItemData(GetItemFromIndex(_currentSelectedSlot)); }

    /// <summary>
    /// Opens the Inventory UI with 'container' as a lootable container (a null 'container' will open a normal inventory)
    /// </summary>
    /// <param name="container">loot container to show contents of in loot scroll view</param>
    public void OpenWithLootContainer(Container container)
    {
        if (_selectedGearIndex != -1)
            CloseGearEquipMenu();

        if (container == null)
        {
            _lootPanel.AddToClassList(_hiddenClass);
            Debug.Log("Hidden");
            _lootPanelOpen = false;
            _selectedLoot = false;

            //reselect inventory panel
            SelectIndex(_currentSelectedSlot).RemoveFromClassList(_selectedClass);
            _selectedLoot = false;
            _currentSelectedSlot = 0;
            SelectIndex(_currentSelectedSlot).AddToClassList(_selectedClass);
        }
        else
        {
            _lootPanel.RemoveFromClassList(_hiddenClass);
            Debug.Log("Shown");
            _lootContainerController.RefreshListWithNewContainer(container);
            _lootContainerSize = container.GetMaxItems();
            _lootPanelOpen = true;

            //reselect loot panel
            SelectIndex(_currentSelectedSlot).RemoveFromClassList(_selectedClass);
            _selectedLoot = true;
            _currentSelectedSlot = 0;
            SelectIndex(_currentSelectedSlot).AddToClassList(_selectedClass);
        }
    }

    public void OnNavigation(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 navDir = context.ReadValue<Vector2>();
            if (_selectedGearIndex != -1)
            {
                //still capture navigation, but no need to navigate no items
                if (_selectableGear.Count > 0)
                {
                    //deselect
                    _gearSelectScrollView.ElementAt(_selectedGearIndex).Q<VisualElement>("Background").RemoveFromClassList(_selectedClass);

                    //move
                    if (navDir.y > 0.5f && _selectedGearIndex > 0)
                        _selectedGearIndex--;
                    else if (navDir.y < -0.5f && _selectedGearIndex < _selectableGear.Count - 1)
                        _selectedGearIndex++;
                    Debug.Log(_selectedGearIndex);

                    //select
                    _gearSelectScrollView.ElementAt(_selectedGearIndex).Q<VisualElement>("Background").AddToClassList(_selectedClass);
                }
            }
            else
            {
                //deselect
                SelectIndex(_currentSelectedSlot).RemoveFromClassList(_selectedClass);

                //move
                _currentSelectedSlot = FindCoreSlotNavigatedTo(_currentSelectedSlot, navDir);

                //select
                SelectIndex(_currentSelectedSlot).AddToClassList(_selectedClass);
                Debug.Log(_currentSelectedSlot);

                if (_currentSelectedSlot >= 0)
                    _inventoryContainerController.ScrollToIndex(_currentSelectedSlot);

                UpdateItemDataCard();
            }
        }
    }

    /// <summary>
    /// finds a new slot index that is in the input direction from the current index within the main Inventory UI
    /// </summary>
    /// <param name="currentSlot">current slot index</param>
    /// <param name="navDir">direction to navigate</param>
    /// <returns>The new slot index</returns>
    int FindCoreSlotNavigatedTo(int currentSlot, Vector2 navDir)
    {
        if (navDir.y > 0.5f)        //up
        {
            if (currentSlot < 0 && currentSlot > -4)
                return currentSlot - 3;
            else if (currentSlot > _itemSlotsPerRow - 1)
                return currentSlot - _itemSlotsPerRow;
        }
        else if (navDir.y < -0.5f)  //down
        {
            if (currentSlot < -3)
                return currentSlot + 3;
            else if (currentSlot >= 0 && ((currentSlot < _inventorySize - _itemSlotsPerRow && !_selectedLoot) || (currentSlot < _lootContainerSize - _itemSlotsPerRow && _selectedLoot)))
                return currentSlot + _itemSlotsPerRow;
        }
        else if (navDir.x > 0.5f)   //right
        {
            //base case
            if ((currentSlot < 0 && (currentSlot + 6) % 3 < 2) || (currentSlot >= 0 && currentSlot % _itemSlotsPerRow < _itemSlotsPerRow - 1))
                return currentSlot + 1;

            //from gear to inventory
            else if (currentSlot == -4)     //secondary
                return 0;
            else if (currentSlot == -1)     //right gear
                return (_itemSlotsPerRow * 2);

            //from inventory to loot
            else if (currentSlot >= 0 && _lootPanelOpen && !_selectedLoot)
            {
                _selectedLoot = true;
                //stay in bounds
                if (currentSlot - (_itemSlotsPerRow - 1) >= _lootContainerSize)
                    return _lootContainerSize - _itemSlotsPerRow;
                else
                    return currentSlot - (_itemSlotsPerRow - 1);
            }
        }
        else if (navDir.x < -0.5f)  //left
        {
            //base case
            if ((currentSlot < 0 && (currentSlot + 6) % 3 > 0) || (currentSlot >= 0 && currentSlot % _itemSlotsPerRow > 0))
                return currentSlot - 1;

            //from loot to inventory
            else if (currentSlot >= 0 && _lootPanelOpen && _selectedLoot)
            {
                _selectedLoot = false;
                //stay in bounds
                if (currentSlot + (_itemSlotsPerRow - 1) >= _inventorySize)
                    return _inventorySize - 1;
                else
                    return currentSlot + (_itemSlotsPerRow - 1);
            }

            //from inventory to gear
            else if (currentSlot <= _itemSlotsPerRow)   //secondary    
                return -4;
            else if (currentSlot > _itemSlotsPerRow)    //right gear
                return -1;
        }
        return currentSlot;
    }

    /// <summary>
    /// Finds the VisualElement of the ItemSlot represented by index
    /// </summary>
    /// <param name="index">index of desired ItemSlot</param>
    /// <returns>VisualElement of ItemSlot at index</returns>
    VisualElement SelectIndex(int index)
    {
        switch (index)
        {
            case -6:
                return _primarySlot;
            case -5:
                return _helmetSlot;
            case -4:
                return _secondarySlot;
            case -3:
                return _leftGadgetSlot;
            case -2:
                return _bodyArmorSlot;
            case -1:
                return _rightGadgetSlot;
            default:
                if (_selectedLoot)
                    return _lootContainerController.GetItemSlotVisualElementAtIndex(index);
                return _inventoryContainerController.GetItemSlotVisualElementAtIndex(index);
        }
    }

    /// <summary>
    /// Gets the Item from the inventory at index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    ItemInstance GetItemFromIndex(int index)
    {
        switch (index)
        {
            case -6:
                return _inventory.primaryWeapon;
            case -5:
                return _inventory.helmet;
            case -4:
                return _inventory.secondaryWeapon;
            case -3:
                return _inventory.leftGadget;
            case -2:
                return _inventory.bodyArmor;
            case -1:
                return _inventory.rightGadget;
            default:
                Container.ItemStack cis;
                if (_selectedLoot)
                    cis = _lootContainerController.GetContainer().GetItemAtIndex(index);
                else
                    cis = _inventory.GetContainer().GetItemAtIndex(index);
                return (cis != null) ? cis.itemInstance : null;

        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        //select gear
        if (_selectedGearIndex != -1)
        {
            ConfirmSelectGear();
        }

        //gear equip
        else if (_currentSelectedSlot < 0)
        {
            StartSelectGear();
        }

        //swap item between containers
        else if (_lootPanelOpen && _currentSelectedSlot >= 0)
        {
            ItemInstance item = GetItemFromIndex(_currentSelectedSlot);
            if (_selectedLoot)
            {
                int success = _inventoryContainerController.GetContainer().AddItem(item);
                if (success == 0)
                    _lootContainerController.GetContainer().RemoveItemAtIndex(_currentSelectedSlot);
            }
            else
            {
                int success = _lootContainerController.GetContainer().AddItem(item);
                if (success == 0)
                    _inventoryContainerController.GetContainer().RemoveItemAtIndex(_currentSelectedSlot);
            }

            UpdateItemDataCard();
        }
    }

    /// <summary>
    /// Starts the gear selection process. It finds the gear that can fit in the currently selected slot and opens the selection UI.
    /// </summary>
    void StartSelectGear()
    {
        Debug.Log("Gear Select");
        ItemInstance currentGear;
        switch (_currentSelectedSlot)
        {
            case -1:            //right gadget
                Debug.Log("Right_Gadget");
                _selectableGear = _inventory.GetContainer().GetItemsForSlot(EGearSlot.Right_Gadget);
                currentGear = _inventory.GetGearSlot(EGearSlot.Right_Gadget);
                break;
            case -2:            //body armor
                Debug.Log("Body_Armor");
                _selectableGear = _inventory.GetContainer().GetItemsForSlot(EGearSlot.Body_Armor);
                currentGear = _inventory.GetGearSlot(EGearSlot.Body_Armor);
                break;
            case -3:            //left gadget
                Debug.Log("Left_Gadget");
                _selectableGear = _inventory.GetContainer().GetItemsForSlot(EGearSlot.Left_Gadget);
                currentGear = _inventory.GetGearSlot(EGearSlot.Left_Gadget);
                break;
            case -4:            //secondary weapon
                Debug.Log("Secondary_Weapon");
                _selectableGear = _inventory.GetContainer().GetItemsForSlot(EGearSlot.Secondary_Weapon);
                currentGear = _inventory.GetGearSlot(EGearSlot.Secondary_Weapon);
                break;
            case -5:            //helmet
                Debug.Log("Helmet");
                _selectableGear = _inventory.GetContainer().GetItemsForSlot(EGearSlot.Helmet);
                currentGear = _inventory.GetGearSlot(EGearSlot.Helmet);
                break;
            case -6:            //primary weapon
                Debug.Log("Primary_Weapon");
                _selectableGear = _inventory.GetContainer().GetItemsForSlot(EGearSlot.Primary_Weapon);
                currentGear = _inventory.GetGearSlot(EGearSlot.Primary_Weapon);
                break;
            default:
                _selectableGear = new List<ItemInstance>();
                currentGear = null;
                break;
        }
        if (currentGear != null)
        {
            _selectableGear.Insert(0, currentGear);
            _gearSlotHasItem = true;
        }
        else
        {
            _gearSlotHasItem = false;
        }
        string debug = "valid: ";
        foreach (var i in _selectableGear)
            debug += i.item.GetItemName() + ", ";
        Debug.Log(debug);
        OpenGearEquipMenu();
    }

    /// <summary>
    /// Finishes the gear selection process. It equips the new gear and closes the selection UI.
    /// </summary>
    void ConfirmSelectGear()
    {
        if (_selectableGear.Count > 0)
        {
            switch (_currentSelectedSlot)
            {
                case -1:            //right gadget
                    Debug.Log("Right_Gadget");
                    EquipGear(EGearSlot.Right_Gadget);
                    break;
                case -2:            //body armor
                    Debug.Log("Body_Armor");
                    EquipGear(EGearSlot.Body_Armor);
                    break;
                case -3:            //left gadget
                    Debug.Log("Left_Gadget");
                    EquipGear(EGearSlot.Left_Gadget);
                    break;
                case -4:            //secondary weapon
                    Debug.Log("Secondary_Weapon");
                    EquipGear(EGearSlot.Secondary_Weapon);
                    break;
                case -5:            //helmet
                    Debug.Log("Helmet");
                    EquipGear(EGearSlot.Helmet);
                    break;
                case -6:            //primary weapon
                    Debug.Log("Primary_Weapon");
                    EquipGear(EGearSlot.Primary_Weapon);
                    break;
                default:
                    break;
            }
        }

        UpdateItemDataCard();

        //stop selecting gear
        CloseGearEquipMenu();
    }

    /// <summary>
    /// puts the selected gear (_selectableGear[_selectedGearIndex]) into the 'gearSlot'
    /// </summary>
    /// <param name="gearSlot"></param>
    void EquipGear(EGearSlot gearSlot)
    {
        //gear is already equipped
        if (_gearSlotHasItem && _selectedGearIndex == 0)
            return;

        ItemInstance currentGear = _inventory.GetGearSlot(gearSlot);
        bool success = _inventory.EquipGear(_selectableGear[_selectedGearIndex], gearSlot);
        if (success)
        {
            _inventory.GetContainer().RemoveItem(_selectableGear[_selectedGearIndex]);
            if (currentGear != null)
                _inventory.GetContainer().AddItem(currentGear);
        }
    }

    /// <summary>
    /// Opens the UI for gear selection and sets the _selectedGearIndex to 0.
    /// </summary>
    void OpenGearEquipMenu()
    {
        //start gear select
        _selectedGearIndex = 0;

        //reveal gear select and hide inventory
        _gearSelectScrollView.RemoveFromClassList(_hiddenClass);
        _inventoryScrollView.AddToClassList(_hiddenClass);

        //populate the gear select scroll view
        _gearSelectScrollView.Clear();
        if (_selectableGear.Count > 0)
        {
            for (int i = 0; i < _selectableGear.Count; i++)
            {
                var gear = _gearSelectEntryTemplate.Instantiate();
                _gearSelectScrollView.Add(gear);
                gear.Q<VisualElement>("ItemIcon").style.backgroundImage = new StyleBackground(_selectableGear[i].item.GetIcon());
                gear.Q<Label>("ItemName").text = _selectableGear[i].item.GetItemName() + ((i == 0 && _gearSlotHasItem) ? " (current)" : "");
                gear.Q<Label>("ItemWeight").text = _selectableGear[i].item.GetWeight() + "kg";
            }

            //first item starts selected
            _gearSelectScrollView.ElementAt(0).Q<VisualElement>("Background").AddToClassList(_selectedClass);
        }
        else
        {
            var notification = _gearSelectNoItemsFitErrorTemplate.Instantiate();
            _gearSelectScrollView.Add(notification);
        }
    }

    /// <summary>
    /// Closes the UI for gear selection and sets the _selectedGearIndex to -1.
    /// </summary>
    void CloseGearEquipMenu()
    {
        //end gear select
        _selectedGearIndex = -1;

        //reveal inventory and hide gear select
        _gearSelectScrollView.AddToClassList(_hiddenClass);
        _inventoryScrollView.RemoveFromClassList(_hiddenClass);
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (_selectedGearIndex == -1)
        {
            GetComponent<HUD>().HideInventoryList();
        }
        else
        {
            CloseGearEquipMenu();
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        GetComponent<HUD>().HideInventoryList();
    }
}
