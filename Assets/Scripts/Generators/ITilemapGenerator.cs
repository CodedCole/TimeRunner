using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITilemapGenerator
{
    public void PrepGenerator(Vector3Int start, int zoneIndex, ZoneGenerator zoneGenerator);

    public IEnumerator Generate();
}
