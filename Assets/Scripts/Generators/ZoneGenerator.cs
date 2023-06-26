using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Zone
{
    public int index;
    public string name;
    public string subtitle;
    public Color color;
    public EGeneratorType[] generator = new EGeneratorType[0];

    public HashSet<Vector3Int> tilesInZone;
    private BoundsInt boundingBox;

    public void GenerateBoundingBox()
    {
        Vector3Int start = tilesInZone.ElementAt(0);
        Vector3Int min = start;
        Vector3Int max = start;

        foreach (var tile in tilesInZone)
        {
            if (min.x > tile.x)
                min.x = tile.x;
            if (min.y > tile.y)
                min.y = tile.y;
            if (min.z > tile.z)
                min.z = tile.z;

            if (max.x < tile.x)
                max.x = tile.x;
            if (max.y < tile.y)
                max.y = tile.y;
            if (max.z < tile.z)
                max.z = tile.z;
        }
        boundingBox = new BoundsInt(min, max - min + Vector3Int.one);
    }

    public BoundsInt GetBoundingBox() { return boundingBox; }
}

public class ZoneGenerator : MonoBehaviour
{
    [SerializeField] private Gradient _zoneColors;
    [SerializeField] private Texture2D _zoneMap;
    [SerializeField] private List<Zone> _zones;
    [SerializeField] private Tilemap _zoneTilemap;
    [SerializeField] private TileBase _tile;
    [SerializeField] private TileBase _wall;

    private Dictionary<Color, int> _colorToZoneIndex = new Dictionary<Color, int>();
    private Color[] _pixels;

    private void Start()
    {
        _pixels = _zoneMap.GetPixels();
        for (int i = 0; i < _pixels.Length; i++)
        {
            if (!_colorToZoneIndex.ContainsKey(_pixels[i]))
            {
                if (_colorToZoneIndex.Count >= _zones.Count)
                {
                    Zone newZone = new Zone();
                    newZone.index = _colorToZoneIndex.Count;
                    newZone.name = "Zone " + _colorToZoneIndex.Count.ToString();
                    newZone.subtitle = "Placeholder Subtitle";
                    newZone.generator = new EGeneratorType[1] { EGeneratorType.Border };
                    _zones.Add(newZone);
                }
                _zones[_colorToZoneIndex.Count].tilesInZone = new HashSet<Vector3Int>();
                _colorToZoneIndex.Add(_pixels[i], _colorToZoneIndex.Count);
                Debug.Log("Found Zone: " + _zones[_colorToZoneIndex[_pixels[i]]].name);
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
                Vector3Int pos = new Vector3Int(i, j, 0);
                int zIndex = _colorToZoneIndex[_pixels[pos.x + (pos.y * _zoneMap.width)]];
                _zones[zIndex].tilesInZone.Add(pos);

                _zoneTilemap.SetTile(pos, _tile);
                _zoneTilemap.SetTileFlags(pos, TileFlags.None);
                _zoneTilemap.SetColor(pos, _zones[zIndex].color);
            }
        }

        foreach (var z in _zones)
        {
            z.GenerateBoundingBox();
            foreach (var gt in z.generator)
            {
                ITilemapGenerator gen = gt.GetGenerator();
                if (gen != null)
                {
                    gen.PrepGenerator(z.index, this);
                    StartCoroutine(gen.Generate());
                }
            }
        }
    }

    public Zone GetZoneAtTile(Vector3Int pos)
    {
        if (pos.x >= _zoneMap.width || pos.x < 0 || pos.y >= _zoneMap.height || pos.y < 0)
            return null;

        return _zones[_colorToZoneIndex[_pixels[pos.x + (pos.y * _zoneMap.width)]]];
    }

    public Zone GetZoneAtIndex(int index)
    {
        if (index >= _zones.Count)
            return null;

        return _zones[index];
    }

    public void BuildWalls(Vector3Int[] positions)
    {
        TileBase[] walls = new TileBase[positions.Length];
        for (int i = 0; i < positions.Length; i++)
            walls[i] = _wall;
        _zoneTilemap.SetTiles(positions, walls);
    }
}
