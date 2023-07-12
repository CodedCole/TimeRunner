using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaveFunctionCollapse;

public class ZoneWFCGenerator : ITilemapGenerator
{
    private int _zoneIndex;
    private ZoneGenerator _zoneGenerator;

    private WFCTemplate _template;

    public void PrepGenerator(int zoneIndex, ZoneGenerator zoneGenerator)
    {
        _zoneIndex = zoneIndex;
        _zoneGenerator = zoneGenerator;
    }

    public void SetTemplate(WFCTemplate template)
    {
        _template = template;
    }

    public IEnumerator Generate()
    {
        Zone zone = _zoneGenerator.GetZoneAtIndex(_zoneIndex);
        TileWFC wfc = new TileWFC(_zoneGenerator.Map, _template, zone.tilesInZone.ToArray(), _zoneGenerator.GetDebugEnabled());

        List<Vector3Int> border = new List<Vector3Int>();
        foreach (var point in zone.border)
        {
            if (zone.tilesInZone.Contains(point))
            {
                foreach(EDirection dir in Enum.GetValues(typeof(EDirection)))
                {
                    if (!zone.tilesInZone.Contains(point + (Vector3Int)dir.GetDirectionVector()))
                    {
                        border.Add(point + (Vector3Int)dir.GetDirectionVector());
                    }
                }
            }
        }
        Dictionary<Vector3Int, HashSet<int>> restrictions = new Dictionary<Vector3Int, HashSet<int>>();
        foreach (var point in border)
        {
            if (!restrictions.ContainsKey(point))
            {
                restrictions.Add(point, new HashSet<int> { 0 });
            }
        }
        wfc.SetTileRestrictions(restrictions);

        yield return _zoneGenerator.StartCoroutine(wfc.GenerateCoroutine(_zoneGenerator));

        yield return null;
    }
}
