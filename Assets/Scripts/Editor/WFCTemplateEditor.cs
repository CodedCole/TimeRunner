using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using WaveFunctionCollapse;

[CustomEditor(typeof(WFCTemplate))]
public class WFCTemplateEditor : Editor
{
    public VisualTreeAsset _inspectorAsset;

    private VisualElement _inspector;
    private ObjectField _sourceField;

    public override VisualElement CreateInspectorGUI()
    {
        _inspector = new VisualElement();

        _inspectorAsset.CloneTree(_inspector);

        _sourceField = _inspector.Q<ObjectField>("source");
        _inspector.Q<Button>("generate-button").clicked += GenerateTemplate;

        return _inspector;
    }

    void GenerateTemplate()
    {
        (target as WFCTemplate).GenerateTemplate(_sourceField.value as Tilemap);
    }
}
