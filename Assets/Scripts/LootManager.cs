using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        HashSet<int> chosen = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            float choice = Random.Range(0.0f, totalWeight);
            int index = -1;
            //compare choice to 1 million-th for the margin of error that comes from subtracting from the float value totalWeight instead of completely recalculating it
            while (choice > 0.0000001f)
            {
                //next item
                index++;

                //check for duplicate
                if (chosen.Contains(index))
                    continue;

                //subtract choice weight of item
                choice -= possibleLoot[index].choiceWeight;
            }

            //add the item to the result
            result[i] = possibleLoot[index].itemStack;

            //don't allow duplicates
            if (!allowDuplicates)
            {
                chosen.Add(index);
                totalWeight -= possibleLoot[index].choiceWeight;
            }
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

    [SerializeField] protected LootProbabilities[] _lootCrates;

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
            if (crate.probabilitiesName == crateTypeName)
            {
                return crate.ChooseLoot();
            }
        }
        return null;
    }

    public static bool TryPutItemInCrate(Item item, string crateTypeName)
    {
        Crate[] crates = Resources.FindObjectsOfTypeAll<Crate>();
        List<Crate> validCrates = new List<Crate>();
        foreach (var crate in crates)
        {
            if (EditorUtility.IsPersistent(crate) || !crate.gameObject.activeInHierarchy)
                continue;

            if (crate.GetCrateTypeName() == crateTypeName)
                validCrates.Add(crate);
        }

        if (validCrates.Count == 0)
            return false;

        validCrates[Random.Range(0, validCrates.Count)].InsertItem(new ItemInitialState() { item=item, stackSize=1 });

        return true;
    }
}
