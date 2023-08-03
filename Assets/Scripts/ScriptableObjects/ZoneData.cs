using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using WaveFunctionCollapse;

[CreateAssetMenu(fileName = "NewZoneData", menuName = "Level/Zone Data")]
public class ZoneData : ScriptableObject
{
    public string zoneName;
    public string subtitle;
    public EGeneratorType[] generator = new EGeneratorType[0];
    public WFCTemplate template;
    public Tile[] doors;
}
