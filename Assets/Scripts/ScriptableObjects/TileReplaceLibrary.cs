using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTileReplacementLibrary", menuName = "Tile Replacement Library")]
public class TileReplaceLibrary : ScriptableObject
{
    [System.Serializable]
    public class TileReplacementPair
    {
        public TileBase tile;
        public GameObject replacement;
    }

    [field: SerializeField] public TileReplacementPair[] Replacements { get; private set; }
}
