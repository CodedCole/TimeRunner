using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorGenerator : ITilemapGenerator
{
    private const int TRY_COUNT = 10;
    private const int TILES_PER_DOOR = 40;
    private const int DISTANCE_BETWEEN_DOORS = 8;

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
        int doorCount = (_zone.tilesInZone.Count / TILES_PER_DOOR) + 1;
        List<Vector3Int> doors = new List<Vector3Int>();
        for (int i = 0; i < doorCount; i++)
        {
            Vector3Int pos = _zone.border.ElementAt(Random.Range(0, _zone.border.Count));
            for (int tries = 0; tries < TRY_COUNT; tries++)
            {
                if (!doors.Contains(pos))
                {
                    bool success = true;
                    foreach(var d in doors)
                    {
                        if ((d - pos).sqrMagnitude < DISTANCE_BETWEEN_DOORS * DISTANCE_BETWEEN_DOORS)
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success && ValidDoorPosition(pos))
                    {
                        doors.AddRange(MakeDoor(pos));
                        break;
                    }
                }

                pos = _zone.border.ElementAt(Random.Range(0, _zone.border.Count));
            }
        }
        _zoneGenerator.MakeEmptySpace(doors.ToArray());
        yield return null;
    }

    private bool ValidDoorPosition(Vector3Int pos)
    {
        //check that the position is within the zone
        if (_zoneGenerator.GetZoneAtTile(pos).index != _zoneIndex)
        {
            return false;
        }

        //check that the position isn't surrounded but is in a wall
        int neighbors = 0;
        EDirection dir = EDirection.North;
        for (int i = 0; i < 4; i++)
        {
            if (/*_zoneGenerator.Map.GetTile(pos + (Vector3Int)dir.GetDirectionVector()) != null*/ _zone.border.Contains(pos + (Vector3Int)dir.GetDirectionVector()))
                neighbors++;
            dir++;
        }
        if (neighbors != 2)
            return false;

        //check that the position is not a corner
        int cornersInZone = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                if (_zoneGenerator.GetZoneAtTile(pos + new Vector3Int(i, j)).index == _zoneIndex)
                    cornersInZone++;
            }
        }
        if (cornersInZone != 5)
            return false;

        return true;
    }

    private Vector3Int[] MakeDoor(Vector3Int pos)
    {
        EDirection dir = EDirection.North;
        bool diagonal = false;
        bool previous = false;
        //check north an extra time
        for (int i = 0; i < 5; i++)
        {
            if (/*_zoneGenerator.Map.GetTile(pos + (Vector3Int)dir.GetDirectionVector()) != null*/ _zone.border.Contains(pos + (Vector3Int)dir.GetDirectionVector()))
            {
                if (previous)
                {
                    diagonal = true;
                }
                previous = true;
            }
            else
            {
                previous = false;
            }
            if (dir != EDirection.West)
                dir++;
            else 
                dir = EDirection.North;
        }
        if (diagonal)
        {
            return new Vector3Int[5] { pos, pos + Vector3Int.up, pos + Vector3Int.down, pos + Vector3Int.left, pos + Vector3Int.right };
        }
        else
        {
            return new Vector3Int[1] { pos };
        }
    }
}
