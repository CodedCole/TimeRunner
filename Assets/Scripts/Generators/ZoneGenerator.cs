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
    public Color color;
    public ZoneData data;

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
    public Gradient _zoneColors;
    [SerializeField] private LevelLayout _levelLayout;
    [SerializeField] private Tilemap _zoneTilemap;
    public TileBase _tile;
    public TileBase _wall;
    public ZoneData _defaultZone;
    [SerializeField] private bool _debug;

    private List<Zone> _zones;
    private Dictionary<Color, int> _colorToZoneIndex = new Dictionary<Color, int>();
    private Color[] _pixels;

    public Tilemap Map { get { return _zoneTilemap; } }

    private void Start()
    {
        //generate zones
        //StartCoroutine(Generate(_levelLayout));
    }

    public IEnumerator Generate(LevelLayout layout, Tilemap tilemap, bool debug = false)
    {
        _levelLayout = layout;
        _zoneTilemap = tilemap;
        _debug = debug;

        //put the serialized zones into the proper place in the zone list
        //Zone[] zones = new Zone[_zones.Count];
        //_zones.CopyTo(zones);
        if(_zones == null)
            _zones = new List<Zone>();
        else
            _zones.Clear();

        foreach (var zone in layout.zones)
        {
            CreateZone(zone);
        }

        //find zones in zone map
        _pixels = layout.map.GetPixels();
        for (int i = 0; i < _pixels.Length; i++)
        {
            if (!_colorToZoneIndex.ContainsKey(_pixels[i]))
            {
                if (_colorToZoneIndex.Count >= _zones.Count)
                {
                    CreateZone(_defaultZone);
                }
                _zones[_colorToZoneIndex.Count].tilesInZone = new HashSet<Vector3Int>();
                _colorToZoneIndex.Add(_pixels[i], _colorToZoneIndex.Count);
                if (_debug)
                    Debug.Log("Found Zone: " + _zones[_colorToZoneIndex[_pixels[i]]].data.zoneName);
            }
        }

        //apply zone colors for debugging
        for (int i = 0; i < _zones.Count; i++)
        {
            _zones[i].color = _zoneColors.Evaluate(((float)i) / _zones.Count);
        }

        //build zones with debug colors
        for (int i = 0; i < layout.map.width; i++)
        {
            for (int j = 0; j < layout.map.height; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                int zIndex = _colorToZoneIndex[_pixels[pos.x + (pos.y * layout.map.width)]];
                _zones[zIndex].tilesInZone.Add(pos);

                _zoneTilemap.SetTile(pos, _tile);
                _zoneTilemap.SetTileFlags(pos, TileFlags.None);
                _zoneTilemap.SetColor(pos, _zones[zIndex].color);
                _zoneTilemap.SetTileFlags(pos, TileFlags.LockColor);
            }
        }

        foreach (var z in _zones)
        {
            z.GenerateBoundingBox();
            if (z.data.generator == null)
                continue;

            foreach (var gt in z.data.generator)
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

    private Zone CreateZone(ZoneData data)
    {
        Zone newZone = new Zone();
        newZone.index = _zones.Count;
        newZone.data = data;
        _zones.Add(newZone);
        return newZone;
    }

    public Zone GetZoneAtTile(Vector3Int pos)
    {
        if (pos.x >= _levelLayout.map.width || pos.x < 0 || pos.y >= _levelLayout.map.height || pos.y < 0)
            return null;

        return _zones[_colorToZoneIndex[_pixels[pos.x + (pos.y * _levelLayout.map.width)]]];
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
