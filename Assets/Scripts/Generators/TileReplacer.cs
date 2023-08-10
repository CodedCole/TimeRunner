using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileReplacer
{
    private Tilemap _targetMap;
    private Dictionary<TileBase, GameObject> _replaceDictionary;
    private Transform _parentForReplacements;

    public TileReplacer(Tilemap targetMap, TileReplaceLibrary replaceLibrary, Transform parentForReplacements)
    {
        _targetMap = targetMap;
        _replaceDictionary = new Dictionary<TileBase, GameObject>();
        foreach(var r in replaceLibrary.Replacements)
        {
            _replaceDictionary.Add(r.tile, r.replacement);
        }
        _parentForReplacements = parentForReplacements;
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
                        GameObject obj = Object.Instantiate(replacement, _targetMap.GetCellCenterWorld(new Vector3Int(x, y, z)) + replacement.transform.position, Quaternion.identity);
                        obj.transform.SetParent(_parentForReplacements);
                        _targetMap.SetTile(new Vector3Int(x, y, z), null);
                    }
                }
            }
        }
    }
}
