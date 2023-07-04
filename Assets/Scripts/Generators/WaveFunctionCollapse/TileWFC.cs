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
        private HashSet<ulong>EMPTY = null;
        private bool _restart = false;

        private Dictionary<Vector3Int, HashSet<int>> _tileRestrictions;

        private bool _debug;

        private WFCTemplate _template;

        public TileWFC(Tilemap input, Tilemap output, Vector2Int offset, Vector2Int size, bool debug = false)
        {
            _input = input;
            _output = output;
            _debug = debug;

            _cells = new Dictionary<Vector3Int, HashSet<ulong>>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    _cells.Add((Vector3Int)offset + new Vector3Int(x, y), new HashSet<ulong>());
                }
            }

            _tileRestrictions = null;

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

            _tileRestrictions = null;
        }

        //INPUT FUNCTIONS
        void CreateTileDataFromInput()
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

        /// <summary>
        /// Sets the what a tile at a cell is allowed to be
        /// </summary>
        /// <param name="restrictions">a dictionary of cell coordinates to sets of tile indexes</param>
        public void SetTileRestrictions(Dictionary<Vector3Int, HashSet<int>> restrictions)
        {
            _tileRestrictions = restrictions;
        }

        //COLLAPSE FUNCTIONS
        /// <summary>
        /// Generates the WFC tilemap
        /// </summary>
        /// <returns></returns>
        public IEnumerator GenerateCoroutine(MonoBehaviour caller)
        {
            //Start
            yield return caller.StartCoroutine(RestartWFC());

            BuildOutputTilemap();
            yield return null;

            //Collapse
            Vector3Int collapsePos = FindCellWithLeastEntropy();
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
                    yield return caller.StartCoroutine(RestartWFC());

                    BuildOutputTilemap();
                    yield return null;
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
        IEnumerator RestartWFC()
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
            if (_tileRestrictions != null)
            {
                List<Vector3Int> cellsRestricted = new List<Vector3Int>();
                foreach (var restriction in _tileRestrictions)
                {
                    int tileIndexInPattern = 0;
                    Vector3Int targetCell = restriction.Key;

                    //if the restriction isn't within the cells, find the first cell that the restriction influences
                    if (!_cells.ContainsKey(restriction.Key))
                    {
                        Vector3Int offset;

                        //check for cells that contian the restriction as a tile within its pattern
                        for (int x = 0; x < _template.PatternSize; x++)
                        {
                            for (int y = 0; y < _template.PatternSize; y++)
                            {
                                offset = new Vector3Int(-x, -y);
                                if (_cells.ContainsKey(restriction.Key + offset))
                                {
                                    targetCell = restriction.Key + offset;
                                    tileIndexInPattern = (x + (y * _template.PatternSize));
                                    break;
                                }
                            }

                            if (targetCell != restriction.Key)
                                break;
                        }

                        //if a cell still isn't found...
                        if (targetCell == restriction.Key)
                        {
                            HashSet<ulong> valid = FindPatternsWithTileRestrictionAtIndex(restriction.Value, 0);
                            bool changed = VirtualPropagate(restriction.Key, valid);
                            if (changed)
                            {
                                Debug.Log("Virtual propagation for " + restriction.Key.ToString());
                            }

                            /*
                            //...then find a neighboring cell that would be influenced if the restriction was actually a cell
                            offset = new Vector3Int(0, 1);
                            if (_cells.ContainsKey(restriction.Key + offset))
                            {
                                targetCell = restriction.Key + offset;
                            }
                            else
                            {
                                offset = new Vector3Int(1, 0);
                                if (_cells.ContainsKey(restriction.Key + offset))
                                {
                                    targetCell = restriction.Key + offset;
                                }
                            }
                            //*/
                        }

                        //if a cell still isn't found...
                        if (targetCell == restriction.Key)
                        {
                            //...then skip this restriction since it cannot be enforced
                            Debug.LogWarning("restriction at " + restriction.Key.ToString() + " could not be resolved");
                            continue;
                        }

                        Debug.Log("Found alternate cell for " + restriction.Key.ToString() + " at " + targetCell.ToString() + " with pattern index at " + tileIndexInPattern.ToString());
                    }


                    //pretend the restriction is a real cell and "collapse" and propagate to neighbors

                    //find all valid patterns with the restriction
                    HashSet<ulong> validWithRestriction = FindPatternsWithTileRestrictionAtIndex(restriction.Value, tileIndexInPattern);

                    //if the target is a cell that doesn't contain the restriction in its pattern,
                    //then get the possible neighbors to the restriction's "cell"
                    if (targetCell == restriction.Key + Vector3Int.right)
                    {
                        validWithRestriction = GetPossibleTilesInDirection(validWithRestriction, EDirection.East);
                    }
                    else if (targetCell == restriction.Key + Vector3Int.up)
                    {
                        validWithRestriction = GetPossibleTilesInDirection(validWithRestriction, EDirection.North);
                    }

                    //apply restriction and propagate
                    validWithRestriction.IntersectWith(_cells[targetCell]);
                    if (validWithRestriction.Count == 0)
                    {
                        Debug.LogError("no possible patterns with restriction at " + restriction.Key.ToString());
                        if (_debug)
                        {
                            _cells[targetCell] = validWithRestriction;
                            BuildOutputTilemap();
                            yield return null;
                        }
                        continue;
                    }
                    else if (!validWithRestriction.SetEquals(_cells[targetCell]))
                    {
                        _cells[targetCell] = validWithRestriction;
                        //Propagate(targetCell);
                        if (_debug)
                        {
                            BuildOutputTilemap();
                            yield return null;
                        }
                    }
                    if (!cellsRestricted.Contains(targetCell))
                        cellsRestricted.Add(targetCell);
                }
                foreach (var c in cellsRestricted)
                {
                    bool changed = Propagate(c);
                    if (_debug && changed)
                    {
                        BuildOutputTilemap();
                        yield return null;
                    }
                }
            }
        }

        HashSet<ulong> FindPatternsWithTileRestrictionAtIndex(HashSet<int> restriction, int index)
        {
            HashSet<ulong> validWithRestriction = new HashSet<ulong>();
            foreach (var pattern in _template.IDtoPattern)
            {
                if (restriction.Contains(pattern.Value.Tiles[index]))
                {
                    validWithRestriction.Add(pattern.Key);
                }
            }
            return validWithRestriction;
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
        /// Calls Propagate at 'pos' after creating a temporary cell at 'pos' with the value 'vcell'. Just propagates if a cell exists at 'pos'.
        /// </summary>
        /// <param name="pos">start position of propagation</param>
        /// <param name="vcell">values to propagate</param>
        /// <returns></returns>
        bool VirtualPropagate(Vector3Int pos, HashSet<ulong> vcell)
        {
            if (!_cells.ContainsKey(pos))
            {
                _cells.Add(pos, vcell);
                bool changed = Propagate(pos);
                _cells.Remove(pos);
                return changed;
            }
            else
            {
                return Propagate(pos);
            }
        }

        /// <summary>
        /// Propagates the WFC rules starting from 'pos'. This could cause an unsolvable tile, which will make the '_restart' flag true
        /// </summary>
        /// <param name="pos">starting position of propagation</param>
        bool Propagate(Vector3Int pos)
        {
            int propTimes = 0;
            bool changed = false;
            //queue and set to track which cells need to be propagated and which already have
            Queue<Vector3Int> cellsToPropagate = new Queue<Vector3Int>();

            //start at pos
            cellsToPropagate.Enqueue(pos);
            while (cellsToPropagate.Count > 0)
            {
                //get the next cell and add it to the completed set
                Vector3Int cellPos = cellsToPropagate.Dequeue();

                //get the cell's possibilities
                HashSet<ulong> cell = GetCellAtPosition(cellPos);

                //propagate each of the directions
                EDirection dir = EDirection.North;
                for (int i = 0; i < 4; i++)
                {
                    Vector3Int neighborPos = cellPos + (Vector3Int)dir.GetDirectionVector();

                    //get neighbor in direction
                    if (_cells.ContainsKey(neighborPos))
                    {
                        propTimes++;
                        //find the possible tiles in the direction 'dir' from the cell
                        HashSet<ulong> newSet = GetPossibleTilesInDirection(cell, dir);

                        //intersect the possible tiles with the neighbor's existing possibilities
                        newSet.IntersectWith(_cells[neighborPos]);

                        //if the neighbor's possibilities and the intersected possibilities match, propagation is not needed on this neighbor
                        if (!_cells[neighborPos].SetEquals(newSet))
                        {
                            changed = true;

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
                                return changed;
                            }
                        }
                    }

                    //next direction (N -> E -> S -> W)
                    dir++;
                }
            }
            Debug.Log(propTimes);
            return changed;
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
