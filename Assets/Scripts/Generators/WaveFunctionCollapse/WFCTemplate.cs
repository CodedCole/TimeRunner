using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    [CreateAssetMenu(fileName = "NewWFCTemplate", menuName = "WFC Template")]
    public class WFCTemplate : ScriptableObject
    {

        //Set in inspector for baking
        [SerializeField] private int _patternSize;
        [SerializeField] private bool _wrapTiles;
        
        private Tilemap _source;

        //baked data
        [SerializeField] private Dictionary<TileBase, int> _tileToIndex = new Dictionary<TileBase, int>();
        [SerializeField] private List<TileBase> _tiles = new List<TileBase>();
        [SerializeField] private Dictionary<ulong, Pattern> _idToPattern = new Dictionary<ulong, Pattern>();

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
            GetTileIndex(null);

            //bake
            _source.CompressBounds();
            FindAllPatterns();
        }

        void FindAllPatterns()
        {
            BoundsInt bounds = _source.cellBounds;
            foreach (var point in bounds.allPositionsWithin)
            {
                Pattern p = BuildPattern(point);
                if (!_idToPattern.ContainsKey(p.PatternID))
                {
                    _idToPattern.Add(p.PatternID, p);
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
            if (_tileToIndex.ContainsKey(tile))
                return _tileToIndex[tile];
            else
            {
                _tileToIndex.Add(tile, _tiles.Count);
                _tiles.Add(tile);
                return _tiles.Count - 1;
            }
        }
    
        
        void FindNeighbors(Pattern pattern)
        {

        }
    }
}
