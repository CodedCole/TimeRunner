using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorGenerator : ITilemapGenerator
{
    private readonly Vector3Int[] NEIGHBOR_DIRECTIONS = new Vector3Int[8]
    {
        Vector3Int.right,
        Vector3Int.right + Vector3Int.down,
        Vector3Int.down,
        Vector3Int.down + Vector3Int.left,
        Vector3Int.left,
        Vector3Int.left + Vector3Int.up,
        Vector3Int.up,
        Vector3Int.up + Vector3Int.right,
    };
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
        _zone.doors = doors.ToHashSet();
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
            if (_zone.border.Contains(pos + (Vector3Int)dir.GetDirectionVector()))
                neighbors++;
            dir++;
        }
        if (neighbors != 2)
            return false;

        //check that the position is not a corner
        neighbors = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                Zone z = _zoneGenerator.GetZoneAtTile(pos + new Vector3Int(i, j));
                if (z != null && z.index == _zoneIndex)
                    neighbors++;
            }
        }
        if (neighbors != 5)
            return false;

        return true;
    }

    private Vector3Int[] MakeDoor(Vector3Int pos)
    {
        int prev = NEIGHBOR_DIRECTIONS.Length - 1;
        int next;
        for (int i = 0; i < NEIGHBOR_DIRECTIONS.Length; i++)
        {
            next = i + 1;
            if (next >= NEIGHBOR_DIRECTIONS.Length)
                next = 0;

            if (_zoneGenerator.GetZoneAtTile(pos + NEIGHBOR_DIRECTIONS[prev]).index != _zoneIndex &&
                _zoneGenerator.GetZoneAtTile(pos + NEIGHBOR_DIRECTIONS[i]).index != _zoneIndex &&
                _zoneGenerator.GetZoneAtTile(pos + NEIGHBOR_DIRECTIONS[next]).index != _zoneIndex)
            {
                if (_zone.data.doors != null && _zone.data.doors.Length == NEIGHBOR_DIRECTIONS.Length)
                    _zoneGenerator.Map.SetTile(pos, _zone.data.doors[i]);
                else
                    _zoneGenerator.Map.SetTile(pos, null);
            }

            prev = i;
        }
        return new Vector3Int[1] { pos };
    }
}
