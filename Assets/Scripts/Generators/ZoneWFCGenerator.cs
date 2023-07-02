using System.Collections;
using System.Collections.Generic;
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
        
        yield return null;
    }
}
