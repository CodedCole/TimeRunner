using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    [CreateAssetMenu(fileName = "NewWFCTemplate", menuName = "WFC Template")]
    public class WFCTemplate : ScriptableObject, ISerializationCallbackReceiver
    {
        //Set in inspector for baking
        [SerializeField] private int _patternSize;
        [SerializeField] private bool _wrapTiles;
        [SerializeField] private TileBase _debugTile;
        
        private Tilemap _source;

        //baked data
        [SerializeField] private List<TileBase> _tiles = new List<TileBase>();
        private Dictionary<TileBase, int> _tileToIndex = new Dictionary<TileBase, int>();
        private Dictionary<ulong, Pattern> _idToPattern = new Dictionary<ulong, Pattern>();

        //serialized data
        [SerializeField] private List<ulong> s_id = new List<ulong>();
        [SerializeField] private List<Pattern> s_pattern = new List<Pattern>();

        //property for external use
        public Dictionary<ulong, Pattern> IDtoPattern { get { return _idToPattern; } }
        public List<TileBase> Tiles { get { return _tiles; } }

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

            //adds the empty tile at index 0
            _tiles.Add(null);

            //bake
            _source.CompressBounds();
            FindAllPatterns();
            FindNeighbors();

            //DEBUG
            Log();
        }

        //patterns
        void FindAllPatterns()
        {
            BoundsInt bounds = _source.cellBounds;
            bounds.min -= new Vector3Int(_patternSize, _patternSize);
            foreach (var point in bounds.allPositionsWithin)
            {
                Pattern p = BuildPattern(point);
                if (!_idToPattern.ContainsKey(p.PatternID))
                {
                    _idToPattern.Add(p.PatternID, p);
                    Debug.Log("new pattern: " + p.PatternID.ToString());
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
                    patternTiles[x + (y * _patternSize)] = GetTileIndex(_source.GetTile(bottomLeft + new Vector3Int(x, y)));
                }
            }
            return new Pattern(patternTiles, _patternSize);
        }

        int GetTileIndex(TileBase tile)
        {
            if (tile == null)
                return 0;
            else if (_tileToIndex.ContainsKey(tile))
                return _tileToIndex[tile];
            else
            {
                _tileToIndex.Add(tile, _tiles.Count);
                _tiles.Add(tile);
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
                    map.SetTile(c.Key, _tiles[p.Tiles[0]]);
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
                    map.SetColor(c.Key, Color.Lerp(Color.blue, Color.red, (float)(c.Value.Count - 1) / _idToPattern.Count));
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

        //Serialize
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
    }
}
