using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootCrateProbabilities
{
    [System.Serializable]
    public struct LootWeight
    {
        public ItemInitialState itemStack;
        public float choiceWeight;
    }

    public AnimationCurve itemCount;
    public bool allowDuplicates;
    public LootWeight[] possibleLoot;

    public ItemInitialState[] ChooseLoot()
    {
        int count = Mathf.CeilToInt(itemCount.Evaluate(Random.Range(0.0f, 1.0f)));
        ItemInitialState[] result = new ItemInitialState[count];

        float totalWeight = 0;
        foreach (var item in possibleLoot)
            totalWeight += item.choiceWeight;

        for (int i = 0; i < count; i++)
        {
            
        }

        return result;
    }
}

public class LootManager : MonoBehaviour
{
    protected static LootManager instance;

    [SerializeField] protected LootCrateProbabilities basicCrate;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }
}
