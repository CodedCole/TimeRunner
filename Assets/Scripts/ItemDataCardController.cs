using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controller for the UIDocument 'ItemDataCard'
/// </summary>
public class ItemDataCardController
{
    VisualElement _root;
    Label _name;
    Label _class;
    Label _weight;
    Label _description;
    VisualElement _icon;

    /// <summary>
    /// Sets up the controller by finding the '#ItemName', '#ItemClass', '#ItemWeight', '#ItemDescription', and '#Item Icon'
    /// </summary>
    /// <param name="root">The root used to find the required Labels and VisualElements</param>
    public ItemDataCardController(VisualElement root)
    {
        _root = root;
        _name = root.Q<Label>("ItemName");
        _class = root.Q<Label>("ItemClass");
        _weight = root.Q<Label>("ItemWeight");
        _description = root.Q<Label>("ItemDescription");
        _icon = root.Q<VisualElement>("ItemIcon");
    }

    /// <summary>
    /// Updates all UI in the ItemDataCard with the data from 'item'.
    /// A null 'item' resets the ItemDataCard
    /// </summary>
    /// <param name="item">data to use</param>
    public void UpdateItemData(Item item)
    {
        if (item != null)
        {
            _name.text = item.GetItemName();
            _class.text = item.GetType().Name;
            _weight.text = item.GetWeight() + "kg";
            _description.text = item.GetDescription();
            _icon.style.backgroundImage = new StyleBackground(item.GetIcon());
        }
        else
        {
            _name.text = "";
            _class.text = "";
            _weight.text = "";
            _description.text = "";
            _icon.style.backgroundImage = null;
        }
    }
}
