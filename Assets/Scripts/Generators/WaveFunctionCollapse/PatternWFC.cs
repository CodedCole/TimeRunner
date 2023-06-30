using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class Pattern : ISerializationCallbackReceiver
    {
        private const int TILE_COUNT_IN_TILESET = 25;

        private int[] _tiles;                   //tiles used in the pattern from bottom left to top right
        private int _size;                      //width and height of the pattern in tiles
        private HashSet<ulong>[] _neighbors;    //patterns that overlap this pattern in each of the four cardinal directions

        //Serialized data
        [Serializable]
        public class Neighbors
        {
            public List<ulong> neighbors = new List<ulong>();
        }

        [SerializeField] private List<Neighbors> s_neighbors = new List<Neighbors>();

        //properties for external use
        public int[] Tiles { get { return _tiles; } }

        public ulong PatternID
        {
            get
            {
                ulong id = 0;
                for (int i = 0; i < _tiles.Length; i++)
                {
                    id += (ulong)(_tiles[i] * Mathf.Pow(TILE_COUNT_IN_TILESET, i));
                }
                return id;
            }
        }

        public Pattern(int[] tiles, int size)
        {
            _tiles = tiles;
            _size = size;
            _neighbors = new HashSet<ulong>[4];
            for (int i = 0; i < 4; i++)
                _neighbors[i] = new HashSet<ulong>();
        }

        public HashSet<ulong> GetNeighborsInDirection(EDirection dir) { return _neighbors[(int)dir]; }

        public int[] GetOverlapInDirection(EDirection dir)
        {
            List<int> overlap = new List<int>();
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    Vector2Int p = new Vector2Int(x, y) + dir.GetDirectionVector();
                    if (p.x < _size && p.x >= 0 && p.y < _size && p.y >= 0)
                        overlap.Add(_tiles[p.x + (p.y * _size)]);
                }
            }
            return overlap.ToArray();
        }

        public override string ToString()
        {
            string result = "PID:" + PatternID.ToString();
            for(int i = 0; i < 4; i++)
            {
                result += "\n" + ((EDirection)i).ToString() + ": ";
                foreach(var neighbor in _neighbors[i])
                {
                    result += neighbor.ToString() + ", ";
                }

            }
            return result;
        }

        public void OnBeforeSerialize()
        {
            s_neighbors.Clear();
            foreach(var direction in _neighbors)
            {
                Neighbors n = new Neighbors();
                foreach (var neighbor in direction)
                {
                    n.neighbors.Add(neighbor);
                }
                s_neighbors.Add(n);
            }
        }

        public void OnAfterDeserialize()
        {
            _neighbors = new HashSet<ulong>[s_neighbors.Count];
            for (int i = 0; i < s_neighbors.Count; i++)
            {
                _neighbors[i] = new HashSet<ulong>();
                foreach (var neighbor in s_neighbors[i].neighbors)
                {
                    _neighbors[i].Add(neighbor);
                }
            }
        }
    }

    public class PatternWFC
    {
        private Tilemap _input;
        private Tilemap _output;
        private int _patternSize;
        private HashSet<Vector3Int> _targetArea;

        private Dictionary<TileBase, int> _tileToIndex;
        private List<TileBase> _tiles;
        private Dictionary<int, Pattern> _possiblePatterns;

        public PatternWFC(Tilemap input, Tilemap output, int patternSize, HashSet<Vector3Int> targetArea)
        {
            _tileToIndex = new Dictionary<TileBase, int>();
            _tiles = new List<TileBase>();
            _possiblePatterns = new Dictionary<int, Pattern>();

            _input = input;
            _output = output;
            _patternSize = patternSize;
            _targetArea = targetArea;

            FindPatterns();
        }

        private void FindPatterns()
        {

        }
    }
}
