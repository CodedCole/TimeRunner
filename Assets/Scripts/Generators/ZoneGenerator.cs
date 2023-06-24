using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Zone
{
    public int index;
    public string name;
    public string subtitle;
    public Color color;
}

public class ZoneGenerator : MonoBehaviour
{
    [SerializeField] private Gradient _zoneColors;
    [SerializeField] private Texture2D _zoneMap;
    [SerializeField] private List<Zone> _zones;
    [SerializeField] private Tilemap _zoneTilemap;
    [SerializeField] private TileBase _tile;

    private Dictionary<Color, Zone> _colorToZone = new Dictionary<Color, Zone>();

    private void Start()
    {
        for(int i = 0; i < _zoneMap.width; i++)
        {
            for (int j = 0; j < _zoneMap.height; j++)
            {
                Color c = _zoneMap.GetPixel(i, j);
                if (!_colorToZone.ContainsKey(c))
                {
                    if (_colorToZone.Count >= _zones.Count)
                    {
                        Zone newZone = new Zone();
                        newZone.index = _colorToZone.Count;
                        newZone.name = "Zone " + _colorToZone.Count.ToString();
                        newZone.subtitle = "Placeholder Subtitle";
                        _zones.Add(newZone);
                    }
                    _colorToZone.Add(c, _zones[_colorToZone.Count]);
                    Debug.Log("Found Zone: " + _colorToZone[c].name);
                }
            }
        }

        for (int i = 0; i < _zones.Count; i++)
        {
            _zones[i].color = _zoneColors.Evaluate(((float)i)/_zones.Count);
        }

        for (int i = 0; i < _zoneMap.width; i++)
        {
            for (int j = 0; j < _zoneMap.height; j++)
            {
                Vector3Int pos = new Vector3Int(i, j);
                _zoneTilemap.SetTile(pos, _tile);
                _zoneTilemap.SetTileFlags(pos, TileFlags.None);
                _zoneTilemap.SetColor(pos, _colorToZone[_zoneMap.GetPixel(i, j)].color);
            }
        }
    }

    public Zone GetZoneAtTile(Vector3Int pos)
    {
        if (pos.x >= _zoneMap.width || pos.x < 0 || pos.y >= _zoneMap.height || pos.y < 0)
            return null;

        return _colorToZone[_zoneMap.GetPixel(pos.x, pos.y)];
    }
}
