using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Color north = Color.red;
    [SerializeField] private Color south = Color.blue;
    [SerializeField] private Color east = Color.green;
    [SerializeField] private Color west = Color.yellow;
    [SerializeField] private Tile tileToFill;
    [SerializeField] private Vector2Int size;

    private Grid _grid;
    private Tilemap _level;

    // Start is called before the first frame update
    void Start()
    {
        _grid = GetComponent<Grid>();
        _level = GetComponentInChildren<Tilemap>();
        _level.ClearAllTiles();
        _level.size = new Vector3Int(size.x, size.y, 2);
        _level.ResizeBounds();
        _level.BoxFill(Vector3Int.zero, tileToFill, 0, 0, size.x, size.y);
        for (int i = 0; i < size.x; i++)
        {
            _level.SetTileFlags(Vector3Int.right * i, TileFlags.None);
            _level.SetColor(Vector3Int.right * i, south);

            _level.SetTileFlags((Vector3Int.right * i) + (Vector3Int.up * (size.y - 1)), TileFlags.None);
            _level.SetColor((Vector3Int.right * i) + (Vector3Int.up * (size.y - 1)), north);
        }
        for (int i = 1; i < size.y - 1; i++)
        {
            _level.SetTileFlags(Vector3Int.up * i, TileFlags.None);
            _level.SetColor(Vector3Int.up * i, west);

            _level.SetTileFlags((Vector3Int.up * i) + (Vector3Int.right * (size.x - 1)), TileFlags.None);
            _level.SetColor((Vector3Int.up * i) + (Vector3Int.right * (size.x - 1)), east);
        }
        //_level.RefreshAllTiles();
    }
}
