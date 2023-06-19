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
        private HashSet<int> EMPTY = null;
        private Vector2Int _size;

        private PriorityQueue<Vector2Int, int> _collapseQueue;
        private int _recurseDepth;

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
            GetTileIndex(null);     //adds the empty tile to the list
            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, min.z);
                    TileBase tile = _input.GetTile(pos);

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
    
        void AddTileInDirection(WFCTileData data, Vector3Int pos, EDirection dir)
        {
            TileBase neighbor = _input.GetTile(pos + (Vector3Int)dir.GetDirectionVector());
            int index = 0;
            if (neighbor != null)
                index = GetTileIndex(neighbor);
            data.neighbors[(int)dir].Add(index);
        }

        int GetTileIndex(TileBase tile)
        {
            if (tile != null && _tileToIndex.ContainsKey(tile))
            {
                return _tileToIndex[tile];
            }
            else if (tile == null && _tileData.Count != 0)
            {
                Debug.Log("empty tile");
                return 0;
            }
            else
            {
                Debug.Log("make tile");
                WFCTileData tileData = new WFCTileData();
                tileData.tile = tile;
                for (int i = 0; i < tileData.neighbors.Length; i++)
                    tileData.neighbors[i] = new HashSet<int>();
                _tileData.Add(tileData);
                if(tile != null)
                    _tileToIndex.Add(tile, _tileData.Count - 1);
                return _tileData.Count - 1;
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
            Vector2Int collapsePos = startPos;
            while (!_collapseQueue.Empty())
            {
                if (collapsePos == -Vector2Int.one)
                    break;

                CollapseCell(collapsePos);

                collapsePos = FindNextCellToCollapse();

                //DEBUG
                _collapseQueue.LogHeap();
                BuildOutputTilemap();
                yield return new WaitForSeconds(0.5f);
            }

            BuildOutputTilemap();
        }

        Vector2Int FindNextCellToCollapse()
        {
            Vector2Int cellToCollapse = -Vector2Int.one;
            int cellEntropy = 0;
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    HashSet<int> cell = GetCellAtPosition(new Vector2Int(x, y));
                    if (cell.Count > 1 && (cell.Count < cellEntropy || cellToCollapse == -Vector2Int.one))
                    {
                        cellToCollapse = new Vector2Int(x, y);
                        cellEntropy = cell.Count;
                    }
                }
            }
            return cellToCollapse;
        }

        void CollapseCell(Vector2Int pos)
        {
            ref HashSet<int> cell = ref GetCellAtPosition(pos);
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

            _recurseDepth = 0;
            Propagate(pos);
            /*
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
            //*/
        }

        /// <summary>
        /// Propagates the WFC rules starting from 'pos'
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        void Propagate(Vector2Int pos)
        {

            ///*
            _recurseDepth++;
            if (_recurseDepth == 10)
            {
                _recurseDepth--;
                return;
            }
            Queue<Vector2Int> cellsToPropagate = new Queue<Vector2Int>();
            HashSet<Vector2Int> cellsAlreadyPropped = new HashSet<Vector2Int>();
            cellsToPropagate.Enqueue(pos);
            while (cellsToPropagate.Count > 0 && cellsToPropagate.Count < 100)
            {
                Vector2Int cellPos = cellsToPropagate.Dequeue();
                cellsAlreadyPropped.Add(cellPos);
                HashSet<int> cell = GetCellAtPosition(cellPos);
                EDirection dir = EDirection.North;
                for (int i = 0; i < 4; i++)
                {
                    Vector2Int neighborPos = cellPos + dir.GetDirectionVector();
                    if (cellsAlreadyPropped.Contains(neighborPos))
                        continue;

                    ref HashSet<int> neighbor = ref GetCellAtPosition(neighborPos);
                    if (neighbor != null)
                    {
                        HashSet<int> newSet = GetPossibleTilesInDirection(cell, dir);
                        newSet.IntersectWith(neighbor);
                        if (newSet != neighbor)
                        {
                            neighbor = newSet;
                            cellsToPropagate.Enqueue(neighborPos);
                            if (newSet.Count == 0)
                            {
                                BuildOutputTilemap();
                                throw new System.Exception("no possible tiles at " + neighborPos.ToString());
                            }
                        }
                    }
                    dir++;
                }
            }
            _recurseDepth--;
            //*/
        }

        ref HashSet<int> GetCellAtPosition(Vector2Int pos)
        {
            //Debug.Log("pos: " + pos.ToString() + " size: " + _size.ToString());
            if (pos.x >= _size.x || pos.x < 0 || pos.y >= _size.y || pos.y < 0)
            {
                //Debug.Log("doesn't exist");
                return ref EMPTY;
            }
            //Debug.Log("exists");
            int index = pos.x + (pos.y * _size.x);
            return ref _cells[index];
        }

        HashSet<int> GetPossibleTilesInDirection(HashSet<int> cell, EDirection dir)
        {
            HashSet<int> result = new HashSet<int>();
            foreach(int tile in cell)
            {
                result.UnionWith(_tileData[tile].neighbors[(int)dir]);
            }
            return result;
        }

        //OUTPUT FUNCTIONS
        void BuildOutputTilemap()
        {
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    ref HashSet<int> cell = ref GetCellAtPosition(pos);
                    if (cell.Count == 1)
                    {
                        int tile = cell.ElementAt(0);
                        _output.SetTile((Vector3Int)pos, _tileData[tile].tile);
                        _output.SetTileFlags((Vector3Int)pos, TileFlags.None);
                        _output.SetColor((Vector3Int)pos, Color.white);
                    }
                    else if (cell.Count == 0)
                    {
                        _output.SetTile((Vector3Int)pos, _tileData[1].tile);
                        _output.SetTileFlags((Vector3Int)pos, TileFlags.None);
                        _output.SetColor((Vector3Int)pos, Color.green);
                    }
                    else
                    {
                        _output.SetTile((Vector3Int)pos, _tileData[1].tile);
                        _output.SetTileFlags((Vector3Int)pos, TileFlags.None);
                        _output.SetColor((Vector3Int)pos, Color.Lerp(Color.blue, Color.red, (float)cell.Count / _tileData.Count));
                    }
                }
            }
        }
        
        //DEBUG FUNCTIONS
        public void LogTileData()
        {
            foreach (var t in _tileData)
            {
                string output = GetTileIndex(t.tile) + " - " + t.tile + "\n" 
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
