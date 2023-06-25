using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTilemapPrefab", menuName = "Tilemap Prefab")]
public class TilemapPrefab : ScriptableObject
{
    public Vector2Int bounds;
    public TileBase[] tiles;

    public void Place(Tilemap map, Vector3Int origin)
    {
        for (int i = 0; i < bounds.y; i++)
        {
            for (int j = 0; j < bounds.x; j++)
            {
                map.SetTile(origin + new Vector3Int(j, i), tiles[j + (i * bounds.x)]);
            }
        }
    }

    public void CreatePrefab()
    {
        Debug.Log("Created Prefab!");
    }
}
