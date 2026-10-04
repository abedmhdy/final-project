using System;
using UnityEngine;

[Serializable]
public class LootEntry
{
    public GameObject pickupPrefab;
    [Min(0)] public int weight = 1;
}

// What an enemy type can drop when it dies. First "dropChance" decides
// whether anything drops at all; then one entry is picked at random, where
// an entry with weight 2 is twice as likely as one with weight 1.
[CreateAssetMenu(fileName = "NewLootTable", menuName = "Dungeon Crawler/Loot Table")]
public class LootTable : ScriptableObject
{
    [Range(0f, 1f)] public float dropChance = 0.2f;
    public LootEntry[] entries;

    // Returns the pickup prefab to spawn, or null for "no drop".
    public GameObject RollDrop()
    {
        if (entries == null || entries.Length == 0) return null;
        if (UnityEngine.Random.value > dropChance) return null;

        int totalWeight = 0;
        foreach (LootEntry entry in entries) totalWeight += entry.weight;
        if (totalWeight <= 0) return null;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (LootEntry entry in entries)
        {
            if (roll < entry.weight) return entry.pickupPrefab;
            roll -= entry.weight;
        }
        return null;
    }
}
