using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Handles spawning of enemies in the game world.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject[] enemyPrefabs;
        public Transform[] spawnPoints;
        public float spawnInterval = 5f;
        public int maxEnemies = 10;
        private readonly HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
        private Coroutine spawnRoutine;

        private void OnEnable()
        {
            spawnRoutine = StartCoroutine(SpawnEnemiesCoroutine());
        }

        private void OnDisable()
        {
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);

            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy == null) continue;
                var baseEnemy = enemy.GetComponent<BaseEnemy>();
                if (baseEnemy != null)
                    baseEnemy.Defeated -= HandleEnemyDefeated;
            }
            activeEnemies.Clear();
        }

        private IEnumerator SpawnEnemiesCoroutine()
        {
            var wait = new WaitForSeconds(spawnInterval);
            while (enabled)
            {
                yield return wait;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (activeEnemies.Count >= maxEnemies)
                return;
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("Enemy spawn skipped: No enemy prefabs assigned.");
                return;
            }
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("Enemy spawn skipped: No spawn points assigned.");
                return;
            }

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (enemyPrefab == null || spawnPoint == null) return;
        
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            activeEnemies.Add(enemy);
        
            BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
                baseEnemy.Defeated += HandleEnemyDefeated;
            else
                Debug.LogError("Spawned enemy is missing BaseEnemy component: " + enemy.name);
        }

        private void HandleEnemyDefeated(BaseEnemy enemy)
        {
            if (enemy == null) return;
            enemy.Defeated -= HandleEnemyDefeated;
            activeEnemies.Remove(enemy.gameObject);
        }
    }
}
