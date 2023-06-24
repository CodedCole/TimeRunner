using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITilemapGenerator
{
    public void PrepGenerator(int zoneIndex, ZoneGenerator zoneGenerator);

    public IEnumerator Generate();
}
