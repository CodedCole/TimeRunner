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
        bool found = false;
        foreach (var point in zone.tilesInZone)
        {
            found = false;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!zone.tilesInZone.Contains(point + new Vector3Int(i, j)))
                    {
                        border.Add(new Vector3Int(point.x + 1, point.y + 1, -1));
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
        }

        _zoneGenerator.BuildWalls(border.ToArray());
        yield return null;
    }
}
