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
        public HashSet<ulong>[] neighbors = new HashSet<ulong>[4];
    }

    public class TileWFC
    {
        private Tilemap _input;
        private Tilemap _output;

        private Dictionary<TileBase, ulong> _tileToIndex = new Dictionary<TileBase, ulong>();
        private List<WFCTileData> _tileData = new List<WFCTileData>();

        private Dictionary<Vector3Int, HashSet<ulong>> _cells;
        private HashSet<ulong> EMPTY = null;
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

            _cells = new Dictionary<Vector3Int, HashSet<ulong>>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    _cells.Add((Vector3Int)offset + new Vector3Int(x, y), new HashSet<ulong>());
                }
            }

            CreateTileDataFromInput();
        }

        public TileWFC(Tilemap output, WFCTemplate template, Vector3Int[] target, bool debug = false)
        {
            _output = output;
            _template = template;
            _debug = debug;

            _cells = new Dictionary<Vector3Int, HashSet<ulong>>();
            foreach(var point in target)
            {
                _cells.Add(point, new HashSet<ulong>());
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

                    WFCTileData data = _tileData[(int)GetTileIndex(tile)];

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
            ulong index = 0;
            if (neighbor != null)
                index = GetTileIndex(neighbor);
            data.neighbors[(int)dir].Add(index);
        }

        ulong GetTileIndex(TileBase tile)
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
                    tileData.neighbors[i] = new HashSet<ulong>();
                _tileData.Add(tileData);
                if(tile != null)
                    _tileToIndex.Add(tile, (ulong)_tileData.Count - 1);
                return (ulong)_tileData.Count - 1;
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
            Vector3Int collapsePos = _cells.Keys.ElementAt(Random.Range(0, _cells.Count));
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
                if (_template != null)
                {
                    foreach(var p in _template.IDtoPattern)
                    {
                        cell.Value.Add(p.Key);
                    }
                }
                else
                {
                    for (int i = 0; i < _tileData.Count; i++)
                    {
                        cell.Value.Add((ulong)i);
                    }
                }
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
            foreach (var cell in _cells)
            {
                //check if cell has least entropy or is first valid cell
                if (cell.Value.Count > 1 && (cell.Value.Count <= cellEntropy || leastEntropyCells.Count == 0))
                {
                    //set cell as the cell with the least entropy
                    if (cell.Value.Count < cellEntropy || leastEntropyCells.Count == 0)
                    {
                        cellEntropy = cell.Value.Count;
                        leastEntropyCells.Clear();
                    }
                    leastEntropyCells.Add(cell.Key);
                }
            }
            //return the cell with least entropy
            if (leastEntropyCells.Count == 0)
                Debug.Log("fully collapsed");
            return (leastEntropyCells.Count == 0 ? -Vector3Int.one : leastEntropyCells[Random.Range(0, leastEntropyCells.Count)]);
        }

        /// <summary>
        /// Forces a cell to collapse and become a specific tile
        /// </summary>
        /// <param name="pos">position of the cell</param>
        void CollapseCell(Vector3Int pos)
        {
            //get cell at 'pos' and check that it exists
            HashSet<ulong> cell = GetCellAtPosition(pos);
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
            ulong collapsedValue = cell.ElementAt(Random.Range(0, cell.Count));
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
                HashSet<ulong> cell = GetCellAtPosition(cellPos);

                //propagate each of the directions
                EDirection dir = EDirection.North;
                for (int i = 0; i < 4; i++)
                {
                    Vector3Int neighborPos = cellPos + (Vector3Int)dir.GetDirectionVector();

                    //check if the neighbor was already propagated
                    //if (cellsAlreadyPropped.Contains(neighborPos))
                    //    continue;

                    //get neighbor in direction
                    if (_cells.ContainsKey(neighborPos))
                    {
                        //find the possible tiles in the direction 'dir' from the cell
                        HashSet<ulong> newSet = GetPossibleTilesInDirection(cell, dir);

                        //intersect the possible tiles with the neighbor's existing possibilities
                        newSet.IntersectWith(_cells[neighborPos]);

                        //if the neighbor's possibilities and the intersected possibilities match, propagation is not needed on this neighbor
                        if (!_cells[neighborPos].SetEquals(newSet))
                        {
                            //apply the intersection to the neighbor
                            _cells[neighborPos] = newSet;

                            //put neighbor in propagation queue
                            if (!cellsToPropagate.Contains(neighborPos))
                                cellsToPropagate.Enqueue(neighborPos);

                            //if the possibility count is 0, then a collision has occured and the '_restart' flag is set
                            if (newSet.Count == 0)
                            {
                                Debug.LogWarning("no possible tiles at " + neighborPos.ToString() + " - restarting");
                                BuildOutputTilemap();
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
        HashSet<ulong> GetCellAtPosition(Vector3Int pos)
        {
            //check that 'pos' is inside '_size', else return the empty set
            if (!_cells.ContainsKey(pos))
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
        HashSet<ulong> GetPossibleTilesInDirection(HashSet<ulong> cell, EDirection dir)
        {
            //union neighbors in 'dir' direction of all possible tiles at 'cell'
            HashSet<ulong> result = new HashSet<ulong>();
            foreach(ulong tile in cell)
            {
                if (_template != null)
                    result.UnionWith(_template.IDtoPattern[tile].GetNeighborsInDirection(dir));
                else
                    result.UnionWith(_tileData[(int)tile].neighbors[(int)dir]);
            }
            return result;
        }

        //OUTPUT FUNCTIONS
        void BuildOutputTilemap()
        {
            if (_template != null)
            {
                _template.Build(_output, _cells);
                return;
            }

            foreach (var cell in _cells)
            {
                if (cell.Value.Count == 1)
                {
                    ulong tile = cell.Value.ElementAt(0);
                    _output.SetTile(cell.Key, _tileData[(int)tile].tile);
                    _output.SetTileFlags(cell.Key, TileFlags.None);
                    _output.SetColor(cell.Key, Color.white);
                }
                else if (cell.Value.Count == 0)
                {
                    _output.SetTile(cell.Key, _tileData[1].tile);
                    _output.SetTileFlags(cell.Key, TileFlags.None);
                    _output.SetColor(cell.Key, Color.green);
                }
                else
                {
                    _output.SetTile(cell.Key, _tileData[1].tile);
                    _output.SetTileFlags(cell.Key, TileFlags.None);
                    _output.SetColor(cell.Key, Color.Lerp(Color.blue, Color.red, (float)cell.Value.Count / _tileData.Count));
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

        string HashSetToString(HashSet<ulong> set)
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
