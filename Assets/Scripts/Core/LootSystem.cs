using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles loot drops from defeated enemies.
/// </summary>
public class LootSystem : MonoBehaviour
{
    [System.Serializable]
    public class LootItem
    {
        public GameObject itemPrefab;
        public float dropChance; // Percentage chance of dropping (0-100)
    }

    private List<LootItem> lootTable = new List<LootItem>();
    [SerializeField] private Transform dropPoint;

    public List<LootItem> LootTable => lootTable; // Exposed read-only property

    private void Awake()
    {
        if (dropPoint == null)
        {
            Debug.LogError("Drop point is not assigned in LootSystem.", this);
        }
    }

    public void DropLoot()
    {
        if (dropPoint == null)
        {
            Debug.LogWarning("Cannot drop loot: Drop point is not assigned.");
            return;
        }
        
        foreach (var loot in lootTable)
        {
            if (loot.itemPrefab == null)
            {
                Debug.LogWarning("Skipping loot drop: itemPrefab is null.");
                continue;
            }
            
            float roll = GetWeightedRandom();
            if (roll <= loot.dropChance)
            {
                Instantiate(loot.itemPrefab, dropPoint.position, Quaternion.identity);
            }
        }
    }

    private float GetWeightedRandom()
    {
        // Generates a weighted random number with a bias towards lower or higher values
        return Mathf.Pow(Random.value, 2) * 100f; // Adjust exponent for different distributions
    }
}
