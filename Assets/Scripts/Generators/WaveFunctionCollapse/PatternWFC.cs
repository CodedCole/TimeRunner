using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    public class Pattern
    {
        private const int TILE_COUNT_IN_TILESET = 25;

        private int[] _tiles;                   //tiles used in the pattern from bottom left to top right
        private int _size;                      //width and height of the pattern in tiles
        private HashSet<int>[] _neighbors;      //patterns that overlap this pattern in each of the four cardinal directions

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
            _neighbors = new HashSet<int>[4];
        }

        public HashSet<int> GetNeighborsInDirection(EDirection dir) { return _neighbors[(int)dir]; }
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
