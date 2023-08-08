using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileReplacer
{
    private Tilemap _targetMap;
    private Dictionary<TileBase, GameObject> _replaceDictionary;

    public TileReplacer(Tilemap targetMap, TileReplaceLibrary replaceLibrary)
    {
        _targetMap = targetMap;
        _replaceDictionary = new Dictionary<TileBase, GameObject>();
        foreach(var r in replaceLibrary.Replacements)
        {
            _replaceDictionary.Add(r.tile, r.replacement);
        }
    }

    public void ReplaceTiles(Vector3Int start, Vector3Int end)
    {
        for (int x = start.x; x <= end.x; x++)
        {
            for (int y = start.y; y <= end.y; y++)
            {
                for (int z = start.z; z <= end.z; z++)
                {
                    TileBase tile = _targetMap.GetTile(new Vector3Int(x, y, z));
                    if (tile != null && _replaceDictionary.TryGetValue(tile, out GameObject replacement))
                    {
                        Object.Instantiate(replacement, _targetMap.GetCellCenterWorld(new Vector3Int(x, y, z)) + replacement.transform.position, Quaternion.identity);
                        _targetMap.SetTile(new Vector3Int(x, y, z), null);
                    }
                }
            }
        }
    }
}
