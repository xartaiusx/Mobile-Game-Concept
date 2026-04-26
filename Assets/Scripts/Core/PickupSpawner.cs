using UnityEngine;

namespace Game.Core
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject goldPickupPrefab;
        [SerializeField] private GameObject healthPotionPickupPrefab;
        [SerializeField] private float goldDropChance = 0.8f;
        [SerializeField] private float potionDropChance = 0.22f;
        [SerializeField] private int minGold = 1;
        [SerializeField] private int maxGold = 4;
        [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.25f, 0f);

        private void OnEnable()
        {
            BaseEnemy.EnemyDefeatedGlobal += HandleEnemyDefeated;
        }

        private void OnDisable()
        {
            BaseEnemy.EnemyDefeatedGlobal -= HandleEnemyDefeated;
        }

        public void SpawnVictoryBonus(Vector3 position)
        {
            Spawn(goldPickupPrefab, position + dropOffset + Vector3.right * 0.4f, Mathf.Max(maxGold, minGold));
            Spawn(healthPotionPickupPrefab, position + dropOffset + Vector3.left * 0.4f, 1);
        }

        private void HandleEnemyDefeated(BaseEnemy enemy)
        {
            if (enemy == null) return;
            Vector3 position = enemy.transform.position + dropOffset;

            if (goldPickupPrefab != null && Random.value <= goldDropChance)
                Spawn(goldPickupPrefab, position + Random.insideUnitSphere * 0.35f, Random.Range(minGold, maxGold + 1));

            if (healthPotionPickupPrefab != null && Random.value <= potionDropChance)
                Spawn(healthPotionPickupPrefab, position + Random.insideUnitSphere * 0.35f, 1);
        }

        private void Spawn(GameObject prefab, Vector3 position, int amount)
        {
            if (prefab == null) return;
            position.y = Mathf.Max(0.4f, position.y);
            GameObject pickup = Instantiate(prefab, position, Quaternion.identity);
            var component = pickup.GetComponent<Pickup>();
            if (component != null)
                component.Configure(component.Item, amount);
        }
    }
}
