using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        bool found;
        foreach (var point in zone.tilesInZone)
        {
            found = false;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!zone.tilesInZone.Contains(point + new Vector3Int(i, j))/* && i != -j/**/)
                    {
                        border.Add(point);
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            /*
            if (!zone.tilesInZone.Contains(point + new Vector3Int(-1, 1)) 
                && zone.tilesInZone.Contains(point + new Vector3Int(-1, 0)) 
                && zone.tilesInZone.Contains(point + new Vector3Int(0, 1)))
            {
                border.Add(new Vector3Int(point.x - 1, point.y + 1));
            }
            if (!zone.tilesInZone.Contains(point + new Vector3Int(1, -1))
                && zone.tilesInZone.Contains(point + new Vector3Int(1, 0))
                && zone.tilesInZone.Contains(point + new Vector3Int(0, -1)))
            {
                border.Add(new Vector3Int(point.x + 1, point.y - 1));
            }
            //*/
        }

        zone.border = border.ToHashSet();
        _zoneGenerator.BuildWalls(border.ToArray());
        yield return null;
    }
}
