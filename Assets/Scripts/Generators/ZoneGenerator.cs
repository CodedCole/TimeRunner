using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using WaveFunctionCollapse;

[Serializable]
public class Zone
{
    public int index;
    public string name;
    public string subtitle;
    public Color color;
    public EGeneratorType[] generator = new EGeneratorType[0];
    public WFCTemplate template;
    public Tile[] doors;

    public HashSet<Vector3Int> tilesInZone;
    public HashSet<Vector3Int> border;
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
    [SerializeField] private bool _debug;

    private Dictionary<Color, int> _colorToZoneIndex = new Dictionary<Color, int>();
    private Color[] _pixels;

    public Tilemap Map { get { return _zoneTilemap; } }

    private void Start()
    {
        //put the serialized zones into the proper place in the zone list
        Zone[] zones = new Zone[_zones.Count];
        _zones.CopyTo(zones);
        _zones.Clear();
        foreach (var zone in zones)
        {
            if (zone.index > _zones.Count)
            {
                while (_zones.Count < zone.index)
                {
                    CreateZone();
                }
            }
            
            if (zone.index == _zones.Count)
            {
                _zones.Add(zone);
            }
            else
            {
                _zones[zone.index] = zone;
            }
        }

        //find zones in zone map
        _pixels = _zoneMap.GetPixels();
        for (int i = 0; i < _pixels.Length; i++)
        {
            if (!_colorToZoneIndex.ContainsKey(_pixels[i]))
            {
                if (_colorToZoneIndex.Count >= _zones.Count)
                {
                    CreateZone();
                }
                _zones[_colorToZoneIndex.Count].tilesInZone = new HashSet<Vector3Int>();
                _colorToZoneIndex.Add(_pixels[i], _colorToZoneIndex.Count);
                if (_debug)
                    Debug.Log("Found Zone: " + _zones[_colorToZoneIndex[_pixels[i]]].name);
            }
        }

        //apply zone colors for debugging
        for (int i = 0; i < _zones.Count; i++)
        {
            _zones[i].color = _zoneColors.Evaluate(((float)i)/_zones.Count);
        }

        //build zones with debug colors
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
                _zoneTilemap.SetTileFlags(pos, TileFlags.LockColor);
            }
        }

        //generate zones
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        foreach (var z in _zones)
        {
            z.GenerateBoundingBox();
            foreach (var gt in z.generator)
            {
                ITilemapGenerator gen = gt.GetGenerator();
                if (gen != null)
                {
                    gen.PrepGenerator(z.index, this);
                    yield return StartCoroutine(gen.Generate());
                }
            }
        }
    }

    private Zone CreateZone()
    {
        Zone newZone = new Zone();
        newZone.index = _zones.Count;
        newZone.name = "Zone " + _zones.Count.ToString();
        newZone.subtitle = "Placeholder Subtitle";
        newZone.generator = new EGeneratorType[2] { EGeneratorType.Border, EGeneratorType.Doors };
        _zones.Add(newZone);
        return newZone;
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
        for (int i = 0; i < positions.Length; i++)
        {
            _zoneTilemap.SetTile(positions[i], _wall);
            _zoneTilemap.SetColor(positions[i], Color.white);
        }
    }

    public void MakeEmptySpace(Vector3Int[] positions)
    {
        TileBase[] emptySpaces = new TileBase[positions.Length];
        for (int i = 0; i < positions.Length; i++)
            emptySpaces[i] = null;
        _zoneTilemap.SetTiles(positions, emptySpaces);
    }

    public bool GetDebugEnabled()
    {
        return _debug;
    }
}
