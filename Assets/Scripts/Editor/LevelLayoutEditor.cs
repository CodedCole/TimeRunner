using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelLayout))]
public class LevelLayoutEditor : Editor
{
    public class ZoneDataListItem
    {
        public Color color;
        public int index;
        public ZoneData zoneData;
    }

    public VisualTreeAsset _inspectorAsset;
    public VisualTreeAsset _zoneFieldAsset;

    private LevelLayout _levelLayout;
    private ObjectField _zoneMapField;
    private VisualElement _zoneMapDisplay;

    private ListView _zoneDataListView;
    private ZoneDataListItem[] _zoneDataListItems;

    public override VisualElement CreateInspectorGUI()
    {
        _levelLayout = (LevelLayout)serializedObject.targetObject;

        VisualElement inspector = new VisualElement();

        _inspectorAsset.CloneTree(inspector);

        _zoneMapField = inspector.Q<ObjectField>("zone-map-image-field");
        _zoneMapDisplay = inspector.Q<VisualElement>("zone-map-display");
        _zoneDataListView = inspector.Q<ListView>("zone-list");

        VisualElement defaultInspector = new VisualElement();
        InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);
        inspector.Q<Foldout>("default-inspector").Add(defaultInspector);

        _zoneMapField.RegisterValueChangedCallback(ZoneMapChanged);

        return inspector;
    }

    private void ZoneMapChanged(ChangeEvent<Object> evt)
    {
        Texture2D map = evt.newValue as Texture2D;
        if (map == null)
            return;

        if (!map.isReadable)
        {
            _zoneMapField.SetValueWithoutNotify(evt.previousValue as Texture2D);
            Debug.LogError("Zone Map must enable read and write");
            return;
        }

        _levelLayout.map = map;
        _zoneMapDisplay.style.backgroundImage = new StyleBackground(map);
        _zoneDataListItems = FindZonesInMap(map);
        if (_levelLayout.zones.Length != _zoneDataListItems.Length)
            _levelLayout.zones = new ZoneData[_zoneDataListItems.Length];

        _zoneDataListView.itemsSource = _zoneDataListItems;
        _zoneDataListView.makeItem = () => {
            VisualElement item = new VisualElement();
            _zoneFieldAsset.CloneTree(item);
            return item;
        };
        _zoneDataListView.bindItem = (VisualElement itemVE, int itemIndex) => {
            VisualElement zColor = itemVE.Q<VisualElement>("zone-color");
            ObjectField zDataField = itemVE.Q<ObjectField>("zone-data-field");

            ZoneDataListItem item = _zoneDataListItems[itemIndex];
            zColor.style.backgroundColor = item.color;
            zDataField.RegisterValueChangedCallback((newZoneData) => {
                ZoneData zd = newZoneData.newValue as ZoneData;
                if (zd != null)
                {
                    _levelLayout.zones[itemIndex] = zd;
                }
                EditorUtility.SetDirty(_levelLayout);
            });
            zDataField.SetValueWithoutNotify(_levelLayout.zones[itemIndex]);
        };
        EditorUtility.SetDirty(_levelLayout);
    }

    private ZoneDataListItem[] FindZonesInMap(Texture2D map)
    {
        Color[] pixels = map.GetPixels();
        List<ZoneDataListItem> result = new List<ZoneDataListItem>();
        Dictionary<Color, int> colorToZoneIndex = new Dictionary<Color, int>();
        foreach (var p in pixels)
        {
            if (!colorToZoneIndex.ContainsKey(p))
            {
                colorToZoneIndex.Add(p, result.Count);
                ZoneDataListItem item = new ZoneDataListItem();
                item.color = p;
                item.index = result.Count;
                result.Add(item);
            }
        }

        return result.ToArray();
    }
}
