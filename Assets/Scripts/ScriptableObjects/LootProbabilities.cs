using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootProbabilities", menuName = "Loot Probabilities")]
public class LootProbabilities : ScriptableObject
{
    [System.Serializable]
    public struct LootWeight
    {
        public ItemInitialState itemStack;
        public float choiceWeight;
    }

    public string probabilitiesName;
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
