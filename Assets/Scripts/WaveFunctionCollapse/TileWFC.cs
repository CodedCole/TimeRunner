using Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

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

        private HashSet<int>[] _cells;
        private Vector2Int _size;

        private PriorityQueue<Vector2Int, int> _collapseQueue;

        public TileWFC(Tilemap input, Tilemap output)
        {
            Debug.Log(input.ToString());
            _input = input;
            _output = output;

            CreateTileDataFromInput();
        }

        //INPUT FUNCTIONS
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

        //COLLAPSE FUNCTIONS
        public IEnumerator GenerateCoroutine()
        {
            _output.ClearAllTiles();

            yield return new WaitForSeconds(0.1f);

            _size = Vector2Int.one * 8; //new Vector2Int(_output.cellBounds.xMax - _output.cellBounds.xMin, _output.cellBounds.yMax - _output.cellBounds.yMin) + Vector2Int.one;
            //_size.x = _output.cellBounds.xMax - _output.cellBounds.xMin;
            //_size.y = _output.cellBounds.yMax - _output.cellBounds.yMin;

            //starting cells can have all values
            _cells = new HashSet<int>[_size.x * _size.y];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new HashSet<int>();
                for (int j = 0; j < _tileData.Count; j++)
                {
                    _cells[i].Add(j);
                }
            }

            //Collapse
            _collapseQueue = new PriorityQueue<Vector2Int, int>(_size.x * _size.y * 4);
            Vector2Int startPos = new Vector2Int(Random.Range(0, _size.x), Random.Range(0, _size.y));
            _collapseQueue.Push(new KeyValuePair<Vector2Int, int>(startPos, 1));
            _collapseQueue.LogHeap();
            while(!_collapseQueue.Empty())
            {
                Vector2Int collapsePos = _collapseQueue.Pop();
                CollapseCell(collapsePos);

                EDirection dir = EDirection.North;
                for (int i = 0; i < 4; i++)
                {
                    HashSet<int> neighbor = GetCellAtPosition(collapsePos + dir.GetDirectionVector());
                    if (neighbor != null && neighbor.Count > 1)
                    {
                        Debug.Log("adding new cell to collapse");
                        _collapseQueue.Push(new KeyValuePair<Vector2Int, int>(collapsePos + dir.GetDirectionVector(), neighbor.Count));
                    }
                    dir++;
                }

                //DEBUG
                _collapseQueue.LogHeap();
                BuildOutputTilemap();
                yield return new WaitForSeconds(0.5f);
            }

            BuildOutputTilemap();
        }

        void CollapseCell(Vector2Int pos)
        {
            HashSet<int> cell = GetCellAtPosition(pos);
            if (cell == null)
            {
                throw new System.Exception("cell at " + pos.ToString() + " is null");
            }
            if (cell.Count <= 0)
            {
                cell.Add(-1);
            }
            int collapsedValue = cell.ElementAt(Random.Range(0, cell.Count));
            
            cell.Clear();
            cell.Add(collapsedValue);

            EDirection dir = EDirection.North;
            for (int i = 0; i < 4; i++)
            {
                HashSet<int> neighbor = GetCellAtPosition(pos + dir.GetDirectionVector());
                if (neighbor != null)
                {

                    neighbor.IntersectWith(_tileData[cell.ElementAt(0)].neighbors[(int)dir]);
                }
                dir++;
            }
        }

        HashSet<int> GetCellAtPosition(Vector2Int pos)
        {
            //Debug.Log("pos: " + pos.ToString() + " size: " + _size.ToString());
            if (pos.x >= _size.x || pos.x < 0 || pos.y >= _size.y || pos.y < 0)
            {
                //Debug.Log("doesn't exist");
                return null;
            }
            //Debug.Log("exists");
            int index = pos.x + (pos.y * _size.x);
            return _cells[index];
        }

        //OUTPUT FUNCTIONS
        void BuildOutputTilemap()
        {
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (GetCellAtPosition(pos).Count == 1)
                    {
                        int tile = GetCellAtPosition(pos).ElementAt(0);
                        _output.SetTile((Vector3Int)pos, (tile == -1 ? null : _tileData[tile].tile));
                    }
                    else
                    {
                        _output.SetTile((Vector3Int)pos, null);
                    }
                }
            }
        }
        
        //DEBUG FUNCTIONS
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
