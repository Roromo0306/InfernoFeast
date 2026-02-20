using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<GameObject, int> seeds = new Dictionary<GameObject, int>();

    public void AddSeeds(GameObject plantPrefab, int amount)
    {
        if (seeds.ContainsKey(plantPrefab))
            seeds[plantPrefab] += amount;
        else
            seeds[plantPrefab] = amount;
    }

    public bool HasSeeds(GameObject plantPrefab)
    {
        return seeds.ContainsKey(plantPrefab) && seeds[plantPrefab] > 0;
    }

    public void UseSeed(GameObject plantPrefab)
    {
        if (HasSeeds(plantPrefab))
            seeds[plantPrefab]--;
    }

    public Dictionary<GameObject, int> GetAvailableSeeds()
    {
        return seeds;
    }
}