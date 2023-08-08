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
            float choice = Random.Range(0.0f, totalWeight);
            int index = -1;
            while(choice > 0)
            {
                index++;
                choice -= possibleLoot[index].choiceWeight;
            }
            result[i] = possibleLoot[index].itemStack;
            Debug.Log("item: " + result[i].item.GetItemName() + ", stack size: " + result[i].stackSize);
        }
        return result;
    }
}

public class LootManager : MonoBehaviour
{
    [System.Serializable]
    public class LootCrateProbabilitiesPair
    {
        public string crateTypeName;
        public LootCrateProbabilities probabilities;
    }

    protected static LootManager instance;

    [SerializeField] protected LootCrateProbabilitiesPair[] _lootCrates;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    public static ItemInitialState[] GetLootForCrateType(string crateTypeName)
    {
        return instance.GetLootForCrateTypeOnInstance(crateTypeName);
    }

    protected ItemInitialState[] GetLootForCrateTypeOnInstance(string crateTypeName)
    {
        foreach (var crate in _lootCrates)
        {
            if (crate.crateTypeName == crateTypeName)
            {
                return crate.probabilities.ChooseLoot();
            }
        }
        return null;
    }
}
