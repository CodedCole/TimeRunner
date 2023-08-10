using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelLayout", menuName = "Level/Level Layout")]
public class LevelLayout : ScriptableObject
{
    public Texture2D map;
    public ZoneData[] zones;
    public TileReplaceLibrary tileReplaceLibrary;
}