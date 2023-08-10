using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelLayout))]
public class LevelLayoutEditor : Editor
{
    public VisualTreeAsset _inspectorAsset;
    public VisualTreeAsset _zoneFieldAsset;

    private ObjectField _zoneMapField;
    private VisualElement _zoneMapDisplay;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();

        _inspectorAsset.CloneTree(inspector);

        _zoneMapField = inspector.Q<ObjectField>("zone-map-image-field");
        _zoneMapDisplay = inspector.Q<VisualElement>("zone-map-display");

        _zoneMapField.RegisterValueChangedCallback(ZoneMapChanged);

        return inspector;
    }

    private void ZoneMapChanged(ChangeEvent<Object> evt)
    {
        Texture2D map = evt.newValue as Texture2D;
        if (map == null)
            return;

        _zoneMapDisplay.style.backgroundImage = new StyleBackground(map);
    }
}
