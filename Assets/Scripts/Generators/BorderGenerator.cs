using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderGenerator : ITilemapGenerator
{
    private ZoneGenerator _zoneGenerator;
    private int _zoneIndex;

    public void PrepGenerator(int zoneIndex, ZoneGenerator zoneGenerator)
    {
        _zoneGenerator = zoneGenerator;
        _zoneIndex = zoneIndex;
    }

    public IEnumerator Generate()
    {
        Zone zone = _zoneGenerator.GetZoneAtIndex(_zoneIndex);

        List<Vector3Int> border = new List<Vector3Int>();
        BoundsInt bounds = zone.GetBoundingBox();
        foreach (var point in zone.tilesInZone)
        {
            EDirection dir = EDirection.North;
            for (int i = 0; i < 4; i++)
            {
                if (!zone.tilesInZone.Contains(point + (Vector3Int)dir.GetDirectionVector()))
                {
                    border.Add(point);
                    break;
                }
                dir++;
            }
        }

        _zoneGenerator.BuildWalls(border.ToArray());
        yield return null;
    }
}
