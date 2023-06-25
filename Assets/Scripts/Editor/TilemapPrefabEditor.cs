using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(TilemapPrefab))]
public class TilemapPrefabEditor : Editor
{
    public VisualTreeAsset _inspector;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();

        _inspector.CloneTree(inspector);

        Button b = inspector.Q<Button>("create-prefab-button");
        b.clicked += (target as TilemapPrefab).CreatePrefab;

        return inspector;
    }
}
