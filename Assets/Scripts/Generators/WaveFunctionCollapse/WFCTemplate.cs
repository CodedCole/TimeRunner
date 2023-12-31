using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    [CreateAssetMenu(fileName = "NewWFCTemplate", menuName = "WFC Template")]
    public class WFCTemplate : ScriptableObject, ISerializationCallbackReceiver
    {
        private readonly Vector3Int COLUMN_OFFSET = new Vector3Int(0, 0, 1);

        [Serializable]
        public class TileColumn
        {
            public TileBase[] tiles;

            public override int GetHashCode()
            {
                int hash = 0;
                for (int i = 0; i < tiles.Length; i++)
                {
                    if (tiles[i] != null)
                        hash += tiles[i].GetHashCode() * (int)Mathf.Pow(32, i);
                }
                return hash;
            }

            public override bool Equals(object obj)
            {
                if (obj is TileColumn)
                {
                    return tiles.SequenceEqual(((TileColumn)obj).tiles);
                }
                return false;
            }
        }

        //Set in inspector for baking
        [SerializeField] private int _patternSize;
        [SerializeField] private bool _wrapTiles;
        [SerializeField] private TileBase _debugTile;
        [SerializeField] private int _columnSize;
        [SerializeField] private int _buildOffset = -1;
        [SerializeField] private TileBase[] _possibleDoorTiles;
        
        private Tilemap _source;

        //baked data
        [SerializeField] private List<TileColumn> _tiles = new List<TileColumn>();
        [SerializeField] private List<int> _doorTiles = new List<int>();
        private Dictionary<TileColumn, int> _tileToIndex = new Dictionary<TileColumn, int>();
        private Dictionary<ulong, Pattern> _idToPattern = new Dictionary<ulong, Pattern>();

        //serialized data
        [SerializeField] private List<ulong> s_id = new List<ulong>();
        [SerializeField] private List<Pattern> s_pattern = new List<Pattern>();

        //property for external use
        public int PatternSize { get { return _patternSize; } }
        public Dictionary<ulong, Pattern> IDtoPattern { get { return _idToPattern; } }
        public List<TileColumn> Tiles { get { return _tiles; } }
        public List<int> Doors { get { return _doorTiles; } }

        public void GenerateTemplate(Tilemap source)
        {
            if (source == null)
            {
                Debug.LogError("A source is required to generate a WFC template");
                return;
            }
            _source = source;

            //clear
            _tileToIndex.Clear();
            _tiles.Clear();
            _idToPattern.Clear();
            _doorTiles.Clear();

            //adds the empty tile at index 0
            TileColumn tc = new TileColumn();
            tc.tiles = new TileBase[_columnSize];
            _tiles.Add(tc);
            _tileToIndex.Add(tc, 0);

            //bake
            _source.CompressBounds();
            FindAllPatterns();
            FindNeighbors();

            EditorUtility.SetDirty(this);

            //DEBUG
            //Log();
        }

        //patterns
        void FindAllPatterns()
        {
            BoundsInt bounds = _source.cellBounds;
            bounds.min -= new Vector3Int(1, 1);
            //bounds.max -= new Vector3Int(1, 1);
            Debug.LogWarning(bounds.zMin);
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    Vector3Int point = new Vector3Int(bounds.xMin + x, bounds.yMin + y, bounds.zMin);
                    Pattern p = BuildPattern(point);
                    if (p.Tiles[0] != 0 || p.Tiles[1] != 0 || p.Tiles[_patternSize] != 0 || p.Tiles[_patternSize + 1] != 0)
                    {
                        if (!_idToPattern.ContainsKey(p.PatternID))
                        {
                            _idToPattern.Add(p.PatternID, p);
                            Debug.Log("new pattern: " + p.PatternID.ToString());
                        }
                    }
                    else
                    {
                        Debug.Log("invalid pattern: " + p.PatternID.ToString());
                    }
                }
            }
        }

        Pattern BuildPattern(Vector3Int bottomLeft)
        {
            int[] patternTiles = new int[_patternSize * _patternSize];
            for (int y = 0; y < _patternSize; y++)
            {
                for (int x = 0; x < _patternSize; x++)
                {
                    patternTiles[x + (y * _patternSize)] = GetTileIndex(bottomLeft + new Vector3Int(x, y));
                }
            }
            return new Pattern(patternTiles, _patternSize);
        }

        int GetTileIndex(Vector3Int tile)
        {
            TileColumn tc = new TileColumn();
            tc.tiles = new TileBase[_columnSize];
            for (int i = 0; i < _columnSize; i++)
                tc.tiles[i] = _source.GetTile(tile + (COLUMN_OFFSET * i));

            //check if this tile column has already been found
            if (_tileToIndex.ContainsKey(tc))
                return _tileToIndex[tc];
            else
            {
                //check if this tile column counts as a door tile
                for (int i = 0; i < _columnSize; i++)
                {
                    if (_possibleDoorTiles.Contains(tc.tiles[i]))
                    {
                        _doorTiles.Add(_tiles.Count);
                        break;
                    }
                }

                //add new tile column
                _tileToIndex.Add(tc, _tiles.Count);
                _tiles.Add(tc);
                return _tiles.Count - 1;
            }
        }
    
        //neighbors
        void FindNeighbors()
        {
            foreach(var p in _idToPattern.Values)
            {
                FindNeighborsOfPattern(p);
            }
        }

        void FindNeighborsOfPattern(Pattern pattern)
        {
            EDirection dir = EDirection.North;
            for (int i = 0; i < 4; i++)
            {
                int[] overlap = pattern.GetOverlapInDirection(dir);
                foreach (var p in _idToPattern.Values)
                {
                    if (overlap.SequenceEqual(p.GetOverlapInDirection(dir.GetOppositeDirection())))
                    {
                        pattern.GetNeighborsInDirection(dir).Add(p.PatternID);
                    }
                }
                dir++;
            }
        }
    
        public void Build(Tilemap map, Dictionary<Vector3Int, HashSet<ulong>> cells)
        {
            map.SetTiles(cells.Keys.ToArray(), null);
            foreach (var c in cells)
            {
                if (c.Value.Count == 1)
                {
                    Pattern p = _idToPattern[c.Value.ElementAt(0)];
                    for (int i = 0; i < _columnSize; i++)
                        map.SetTile(c.Key + (COLUMN_OFFSET * (i + _buildOffset)), _tiles[p.Tiles[0]].tiles[i]);
                }
                else if (c.Value.Count == 0)
                {
                    map.SetTile(c.Key, _debugTile);
                    map.SetTileFlags(c.Key, TileFlags.None);
                    map.SetColor(c.Key, Color.green);
                }
                else
                {
                    map.SetTile(c.Key, _debugTile);
                    map.SetTileFlags(c.Key, TileFlags.None);
                    map.SetColor(c.Key, Color.Lerp(Color.red, Color.blue, (float)(c.Value.Count - 1) / _idToPattern.Count));
                }
            }
        }

        //DEBUG
        public void Log()
        {
            if (_idToPattern.Count == 0)
                Debug.LogWarning("no patterns in template");
            foreach(var p in _idToPattern)
            {
                Debug.Log(p.Value.ToString());
            }
        }

        #region Serialization
        public void OnBeforeSerialize()
        {
            s_id.Clear();
            s_pattern.Clear();
            foreach (var p in _idToPattern)
            {
                s_id.Add(p.Key);
                s_pattern.Add(p.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            _idToPattern = new Dictionary<ulong, Pattern>();
            for(int i = 0; i < s_id.Count; i++)
            {
                _idToPattern.Add(s_id[i], s_pattern[i]);
            }
        }
        #endregion
    }
}
