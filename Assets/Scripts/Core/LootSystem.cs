using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Handles loot drops from defeated enemies.
    /// </summary>
    public class LootSystem : MonoBehaviour
    {
        [System.Serializable]
        public class LootItem
        {
            public GameObject itemPrefab;
            [Range(0f, 100f)] public float dropChance;
        }

        [SerializeField] private List<LootItem> lootTable = new List<LootItem>();
        [SerializeField] private Transform dropPoint;

        public IReadOnlyList<LootItem> LootTable => lootTable;

        private void OnEnable()
        {
            BaseEnemy.EnemyDefeatedGlobal += HandleEnemyDefeated;
        }

        private void OnDisable()
        {
            BaseEnemy.EnemyDefeatedGlobal -= HandleEnemyDefeated;
        }

        private void HandleEnemyDefeated(BaseEnemy enemy)
        {
            DropLoot(enemy != null ? enemy.transform.position : transform.position);
        }

        public void DropLoot()
        {
            DropLoot(dropPoint != null ? dropPoint.position : transform.position);
        }

        public void DropLoot(Vector3 position)
        {
            if (lootTable == null) return;

            foreach (var loot in lootTable)
            {
                if (loot == null || loot.itemPrefab == null)
                    continue;
            
                if (Random.value * 100f <= loot.dropChance)
                    Instantiate(loot.itemPrefab, position, Quaternion.identity);
            }
        }
    }
}
