using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorGenerator : ITilemapGenerator
{
    private int _zoneIndex;
    private ZoneGenerator _zoneGenerator;
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
