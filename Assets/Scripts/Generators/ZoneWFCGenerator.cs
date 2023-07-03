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

    public IEnumerator Generate()
    {
        Zone zone = _zoneGenerator.GetZoneAtIndex(_zoneIndex);
        TileWFC wfc = new TileWFC(_zoneGenerator.Map, _template, zone.tilesInZone.ToArray(), true);

        yield return null;
    }
}
