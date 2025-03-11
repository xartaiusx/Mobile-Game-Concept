using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles spawning of enemies in the game world.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    public int maxEnemies = 10;
    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    private IEnumerator SpawnEnemiesCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (activeEnemies.Count >= maxEnemies)
        {
            Debug.LogWarning("Enemy spawn skipped: Max enemy limit reached.");
            return;
        }
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Enemy spawn skipped: No enemy prefabs assigned.");
            return;
        }
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Enemy spawn skipped: No spawn points assigned.");
            return;
        }

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        activeEnemies.Add(enemy);
        
        BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
        if (baseEnemy != null)
        {
            baseEnemy.OnEnemyDefeated += HandleEnemyDefeated;
        }
        else
        {
            Debug.LogError("Spawned enemy is missing BaseEnemy component: " + enemy.name);
        }
    }

    private void HandleEnemyDefeated(BaseEnemy enemy)
    {
        activeEnemies.Remove(enemy.gameObject);
        Destroy(enemy.gameObject);
    }
}
