using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteriorWallGenerator : ITilemapGenerator
{
    private int _zoneIndex;
    private ZoneGenerator _zoneGenerator;
    private Zone _zone;

    public void PrepGenerator(int zoneIndex, ZoneGenerator zoneGenerator)
    {
        _zoneIndex = zoneIndex;
        _zoneGenerator = zoneGenerator;
        _zone = zoneGenerator.GetZoneAtIndex(zoneIndex);
    }

    public IEnumerator Generate()
    {
        yield return null;
    }
}
