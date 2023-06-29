using Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<Vector3Int, HashSet<int>> _cells;
        private HashSet<int> EMPTY = null;
        private Vector2Int _offset;
        private Vector2Int _size;
        private bool _restart = false;

        private bool _debug;

        private WFCTemplate _template;

        public TileWFC(Tilemap input, Tilemap output, Vector2Int offset, Vector2Int size, bool debug = false)
        {
            _input = input;
            _output = output;
            _offset = offset;
            _size = size;
            _debug = debug;

            _cells = new Dictionary<Vector3Int, HashSet<int>>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    _cells.Add((Vector3Int)offset + new Vector3Int(x, y), new HashSet<int>());
                }
            }

            CreateTileDataFromInput();
        }

        public TileWFC(WFCTemplate template, Vector3Int[] target, bool debug = false)
        {
            _template = template;
            _debug = debug;

            _cells = new Dictionary<Vector3Int, HashSet<int>>();
            foreach(var point in target)
            {
                _cells.Add(point, new HashSet<int>());
            }
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
                return 0;
            }
            else
            {
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
        /// <summary>
        /// Generates the WFC tilemap
        /// </summary>
        /// <returns></returns>
        public IEnumerator GenerateCoroutine()
        {
            //Start
            RestartWFC();

            //Collapse
            Vector3Int collapsePos = new Vector3Int(Random.Range(0, _size.x), Random.Range(0, _size.y));
            while (true)
            {
                if (collapsePos == -Vector3Int.one)
                    break;

                CollapseCell(collapsePos);
                if (_restart)
                {
                    if (_debug)
                        yield return null;

                    _restart = false;
                    RestartWFC();
                }


                collapsePos = FindCellWithLeastEntropy();

                //DEBUG
                if (_debug)
                {
                    BuildOutputTilemap();
                    yield return null;
                }
            }

            //Output
            BuildOutputTilemap();
            _cells = null;
        }

        /// <summary>
        /// Prepares WFC to start or restart
        /// </summary>
        void RestartWFC()
        {
            //clear the output of tiles
            _output.SetTiles(_cells.Keys.ToArray(), null);

            //starting cells can have all values
            foreach (var cell in _cells)
            {
                cell.Value.Clear();

            }
        }

        /// <summary>
        /// Finds the cell with the least entropy
        /// </summary>
        /// <returns>grid position of cell with lowest entropy. -Vector2Int.one is returned when all cells are collapsed.</returns>
        Vector3Int FindCellWithLeastEntropy()
        {
            //prepare for finding the smallest
            List<Vector3Int> leastEntropyCells = new List<Vector3Int>();
            int cellEntropy = 0;
            foreach (var point in _cells.Keys)
            {
                    //check if cell has least entropy or is first valid cell
                    HashSet<int> cell = GetCellAtPosition(point);
                    if (cell.Count > 1 && (cell.Count <= cellEntropy || leastEntropyCells.Count == 0))
                    {
                        //set cell as the cell with the least entropy
                        if (cell.Count < cellEntropy || leastEntropyCells.Count == 0)
                        {
                            cellEntropy = cell.Count;
                            leastEntropyCells.Clear();
                        }
                        leastEntropyCells.Add(point);
                    }
            }
            //return the cell with least entropy
            return (leastEntropyCells.Count == 0 ? -Vector3Int.one : leastEntropyCells[Random.Range(0, leastEntropyCells.Count)]);
        }

        /// <summary>
        /// Forces a cell to collapse and become a specific tile
        /// </summary>
        /// <param name="pos">position of the cell</param>
        void CollapseCell(Vector3Int pos)
        {
            //get cell at 'pos' and check that it exists
            HashSet<int> cell = GetCellAtPosition(pos);
            if (cell == null)
            {
                throw new System.Exception("cell at " + pos.ToString() + " is null");
            }

            //check that the cell is not a collision
            if (cell.Count <= 0)
            {
                _restart = true;
                return;
            }

            //collapse cell to a single value
            int collapsedValue = cell.ElementAt(Random.Range(0, cell.Count));
            cell.Clear();
            cell.Add(collapsedValue);

            //propagate this change to the neighbors
            Propagate(pos);
        }

        /// <summary>
        /// Propagates the WFC rules starting from 'pos'. This could cause an unsolvable tile, which will make the '_restart' flag true
        /// </summary>
        /// <param name="pos">starting position of propagation</param>
        void Propagate(Vector3Int pos)
        {
            //queue and set to track which cells need to be propagated and which already have
            Queue<Vector3Int> cellsToPropagate = new Queue<Vector3Int>();
            HashSet<Vector3Int> cellsAlreadyPropped = new HashSet<Vector3Int>();

            //start at pos
            cellsToPropagate.Enqueue(pos);
            while (cellsToPropagate.Count > 0)
            {
                //get the next cell and add it to the completed set
                Vector3Int cellPos = cellsToPropagate.Dequeue();
                cellsAlreadyPropped.Add(cellPos);

                //get the cell's possibilities
                HashSet<int> cell = GetCellAtPosition(cellPos);

                //propagate each of the directions
                EDirection dir = EDirection.North;
                for (int i = 0; i < 4; i++)
                {
                    Vector3Int neighborPos = cellPos + (Vector3Int)dir.GetDirectionVector();

                    //check if the neighbor was already propagated
                    //if (cellsAlreadyPropped.Contains(neighborPos))
                    //    continue;

                    //get neighbor in direction
                    HashSet<int> neighbor = GetCellAtPosition(neighborPos);
                    if (neighbor != null)
                    {
                        //find the possible tiles in the direction 'dir' from the cell
                        HashSet<int> newSet = GetPossibleTilesInDirection(cell, dir);

                        //intersect the possible tiles with the neighbor's existing possibilities
                        newSet.IntersectWith(neighbor);

                        //if the neighbor's possibilities and the intersected possibilities match, propagation is not needed on this neighbor
                        if (!neighbor.SetEquals(newSet))
                        {
                            //apply the intersection to the neighbor
                            neighbor = newSet;

                            //put neighbor in propagation queue
                            if (!cellsToPropagate.Contains(neighborPos))
                                cellsToPropagate.Enqueue(neighborPos);

                            //if the possibility count is 0, then a collision has occured and the '_restart' flag is set
                            if (newSet.Count == 0)
                            {
                                BuildOutputTilemap();
                                Debug.LogWarning("no possible tiles at " + neighborPos.ToString() + " - restarting");
                                _restart = true;
                                return;
                            }
                        }
                    }

                    //next direction (N -> E -> S -> W)
                    dir++;
                }
            }
        }

        /// <summary>
        /// Gets the set of possible tiles for a cell at 'pos'
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>HashSet of the indexes for each possible tile</returns>
        HashSet<int> GetCellAtPosition(Vector3Int pos)
        {
            //check that 'pos' is inside '_size', else return the empty set
            if (pos.x >= _size.x || pos.x < 0 || pos.y >= _size.y || pos.y < 0)
                return EMPTY;

            //return the cell possibility set at 'pos'
            return _cells[pos];
        }

        /// <summary>
        /// Finds the set of pssible tiles that could be at a neighbor of 'cell' in direction 'dir'
        /// </summary>
        /// <param name="cell">set of possible tiles, which is used to find the neighbor's possible tiles</param>
        /// <param name="dir">direction from 'cell' to find the possible tiles</param>
        /// <returns>HashSet of the possible tiles in direction 'dir'</returns>
        HashSet<int> GetPossibleTilesInDirection(HashSet<int> cell, EDirection dir)
        {
            //union neighbors in 'dir' direction of all possible tiles at 'cell'
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
            foreach (var point in _cells.Keys)
            {
                    HashSet<int> cell = GetCellAtPosition(point);
                    if (cell.Count == 1)
                    {
                        int tile = cell.ElementAt(0);
                        _output.SetTile(point + (Vector3Int)_offset, _tileData[tile].tile);
                        _output.SetTileFlags(point + (Vector3Int)_offset, TileFlags.None);
                        _output.SetColor(point + (Vector3Int)_offset, Color.white);
                    }
                    else if (cell.Count == 0)
                    {
                        _output.SetTile(point + (Vector3Int)_offset, _tileData[1].tile);
                        _output.SetTileFlags(point + (Vector3Int)_offset, TileFlags.None);
                        _output.SetColor(point + (Vector3Int)_offset, Color.green);
                    }
                    else
                    {
                        _output.SetTile(point + (Vector3Int)_offset, _tileData[1].tile);
                        _output.SetTileFlags(point + (Vector3Int)_offset, TileFlags.None);
                        _output.SetColor(point + (Vector3Int)_offset, Color.Lerp(Color.blue, Color.red, (float)cell.Count / _tileData.Count));
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
