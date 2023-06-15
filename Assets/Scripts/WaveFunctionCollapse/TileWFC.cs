using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            Debug.Log(input.ToString());
            _input = input;
            _output = output;

            CreateTileDataFromInput();
        }

        public void CreateTileDataFromInput()
        {
            _input.CompressBounds();
            Vector3Int min = _input.cellBounds.min;
            Vector3Int max = _input.cellBounds.max;
            Debug.Log("min: " + min.ToString() + " max: " + max.ToString());
            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, min.z);
                    TileBase tile = _input.GetTile(pos);
                    if (tile != null)
                    {
                        WFCTileData data = _tileData[GetTileIndex(tile)];

                        //North
                        AddTileInDirection(data, pos, EDirection.North);

                        //East
                        AddTileInDirection(data, pos, EDirection.East);

                        //South
                        AddTileInDirection(data, pos, EDirection.South);

                        //West
                        AddTileInDirection(data, pos, EDirection.West);
                    }
                }
            }
        }
    
        void AddTileInDirection(WFCTileData data, Vector3Int pos, EDirection dir)
        {
            TileBase neighbor = _input.GetTile(pos + (Vector3Int)dir.GetDirectionVector());
            int index = -1;
            if (neighbor != null)
                index = GetTileIndex(neighbor);
            data.neighbors[(int)dir].Add(index);
        }

        int GetTileIndex(TileBase tile)
        {
            if (_tileToIndex.ContainsKey(tile))
            {
                return _tileToIndex[tile];
            }
            else
            {
                WFCTileData tileData = new WFCTileData();
                tileData.tile = tile;
                for (int i = 0; i < tileData.neighbors.Length; i++)
                    tileData.neighbors[i] = new HashSet<int>();
                _tileData.Add(tileData);
                _tileToIndex.Add(tile, _tileToIndex.Count);
                return _tileToIndex.Count - 1;
            }
        }

        public void LogTileData()
        {
            foreach (var t in _tileData)
            {
                string output = _tileToIndex[t.tile] + " - " + t.tile.ToString() + "\n" 
                    + "N: " + HashSetToString(t.neighbors[(int)EDirection.North]) + "\n"
                    + "E: " + HashSetToString(t.neighbors[(int)EDirection.East]) + "\n"
                    + "S: " + HashSetToString(t.neighbors[(int)EDirection.South]) + "\n"
                    + "W: " + HashSetToString(t.neighbors[(int)EDirection.West])+ "\n";
                Debug.Log(output);
            }
        }

        string HashSetToString(HashSet<int> set)
        {
            string output = "";
            foreach (var element in set)
            {
                output += element.ToString() + ", ";
            }
            return output;
        }
    }
}
