using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    public class WFCTileData
    {
        public TileBase tile;
        public HashSet<int>[] neighbors = new HashSet<int>[4];
    }

    public class TileWFC
    {
        private Tilemap _input;
        private Tilemap _output;

        private Dictionary<TileBase, int> _tileToIndex = new Dictionary<TileBase, int>();
        private List<WFCTileData> _tileData = new List<WFCTileData>();

        public TileWFC(Tilemap input, Tilemap output)
        {
            _input = input;
            _output = output;
        }

        public void CreateTileDataFromInput()
        {
            Vector3Int min = _input.cellBounds.min;
            Vector3Int max = _input.cellBounds.max;

            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    TileBase tile = _input.GetTile(new Vector3Int(x, y, min.z));
                    if (tile != null)
                    {
                        if (!_tileToIndex.ContainsKey(tile))
                        {
                            _tileToIndex.Add(tile, _tileToIndex.Count);

                            WFCTileData tileData = new WFCTileData();
                            tileData.tile = tile;
                            for (int i = 0; i < tileData.neighbors.Length; i++)
                                tileData.neighbors[i] = new HashSet<int>();

                            _tileData.Add(tileData);
                        }
                        WFCTileData data = _tileData[_tileToIndex[tile]];

                        //North
                        TileBase neighbor = _input.GetTile(new Vector3Int(x, y + 1, min.z));
                        int index = -1;
                        if (neighbor != null)
                            index = _tileToIndex[neighbor];
                        data.neighbors[(int)EDirection.North].Add(index);

                        //East
                        neighbor = _input.GetTile(new Vector3Int(x + 1, y, min.z));
                        index = -1;
                        if (neighbor != null)
                            index = _tileToIndex[neighbor];
                        data.neighbors[(int)EDirection.East].Add(index);

                        //South
                        neighbor = _input.GetTile(new Vector3Int(x, y - 1, min.z));
                        index = -1;
                        if (neighbor != null)
                            index = _tileToIndex[neighbor];
                        data.neighbors[(int)EDirection.South].Add(index);

                        //West
                        neighbor = _input.GetTile(new Vector3Int(x - 1, y, min.z));
                        index = -1;
                        if (neighbor != null)
                            index = _tileToIndex[neighbor];
                        data.neighbors[(int)EDirection.West].Add(index);
                    }
                }
            }
        }
    }
}
