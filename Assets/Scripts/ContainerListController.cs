using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ContainerListController
{
    private VisualTreeAsset _itemSlotTemplate;
    private ScrollView _scrollView;

    private Container _container;

    private ItemSlot[] _itemSlots = new ItemSlot[0];
    private int _slotsPerRow;

    public void InitializeList(ScrollView scrollView, VisualTreeAsset template, ref Container container, int slotsPerRow)
    {
        _itemSlotTemplate = template;
        _container = container;

        _scrollView = scrollView;
        _slotsPerRow = slotsPerRow;

        GenerateList();

        RegisterForUpdateUI();

        UpdateUI();
    }

    /// <summary>
    /// Updates the UI to use a different container and re-registers for events
    /// </summary>
    /// <param name="container"></param>
    public void RefreshListWithNewContainer(Container container)
    {
        //re-register with add and remove events
        //unregister old container
        UnregisterForUpdateUI();

        //set new container
        _container = container;

        GenerateList();

        //register new container
        RegisterForUpdateUI();

        UpdateUI();
    }

    void GenerateList()
    {
        if (_itemSlots.Length > 0)
        {
            //remove all current item slots
            _scrollView.Clear();
        }

        //create item slots
        _itemSlots = new ItemSlot[_container.GetMaxItems()];
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            //make new slot VisualElement
            var slot = _itemSlotTemplate.Instantiate();
            _scrollView.Add(slot);

            //setup ItemSlot
            _itemSlots[i] = new ItemSlot();
            _itemSlots[i].SetVisualElements(slot);
            _itemSlots[i].UpdateSize(_slotsPerRow);
        }
    }

    void RegisterForUpdateUI()
    {
        _container.RegisterItemAddEvent(UpdateUI);
        _container.RegisterItemRemovedEvent(UpdateUI);
    }

    void UnregisterForUpdateUI()
    {
        _container.UnregisterItemAddEvent(UpdateUI);
        _container.UnregisterItemRemovedEvent(UpdateUI);
    }

    /// <summary>
    /// Updates all item slots with the correct item data
    /// </summary>
    public void UpdateUI()
    {
        for (int i = 0; i < _itemSlots.Length; i++)
            _itemSlots[i].SetItemData(_container.GetItemAtIndex(i));
    }

    /// <summary>
    /// Calls ItemSlot.UpdateSize() on all item slots
    /// </summary>
    public void UpdateSize()
    {
        foreach (var i in _itemSlots)
            i.UpdateSize(_slotsPerRow);
    }

    public Container GetContainer() { return _container; }

    public VisualElement GetItemSlotVisualElementAtIndex(int index) { return _itemSlots[index].GetVisualElement(); }

    public void ScrollToIndex(int index) { _scrollView.ScrollTo(GetItemSlotVisualElementAtIndex(index)); }
}

public class ItemSlot
{
    VisualElement _icon;
    Label _stackSize;
    VisualElement _root;

    /// <summary>
    /// Sets all VisualElement references in the ItemSlot from a 'root' VisualElement
    /// </summary>
    /// <param name="root"></param>
    public void SetVisualElements(VisualElement root)
    {
        _root = root;
        _icon = root.Q<VisualElement>("Icon");
        _stackSize = root.Q<Label>("StackSize");
    }

    /// <summary>
    /// Set VisualElements of the item slot to reflect the data from an item
    /// </summary>
    /// <param name="item"></param>
    public void SetItemData(Container.ItemStack item)
    {
        if (item != null)
        {
            _icon.style.backgroundImage = new StyleBackground(item.itemInstance.item.GetIcon());
            if (item.count > 1)
                _stackSize.text = item.count.ToString();
            else
                _stackSize.text = "";
        }
        else
        {
            _icon.style.backgroundImage = null;
            _stackSize.text = "";
        }
    }

    /// <summary>
    /// Updates width and height of the root VisualElement to maintain a square shape for the item slot
    /// </summary>
    /// <param name="slotsPerRow">The number of item slots that fit on each row</param>
    public void UpdateSize(int slotsPerRow)
    {
        _root.style.minWidth = new StyleLength(Length.Percent((100.0f / slotsPerRow) - 1));
        _root.style.minHeight = _root.localBound.width < 10 ? 20 : _root.localBound.width;
    }

    public VisualElement GetVisualElement() { return _icon; }
}
