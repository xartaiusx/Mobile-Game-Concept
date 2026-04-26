using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public enum ArenaState
    {
        Preparing,
        Wave,
        Boss,
        Victory,
        Failure
    }

    public class ArenaController : MonoBehaviour
    {
        [SerializeField] private BaseCharacter player;
        [SerializeField] private BaseEnemy[] initialWaveEnemies;
        [SerializeField] private GameObject bossObject;
        [SerializeField] private GameObject[] waveEnemyPrefabs;
        [SerializeField] private Transform[] waveSpawnPoints;
        [SerializeField] private int enemiesPerWave = 2;
        [SerializeField] private int waveCount = 1;
        [SerializeField] private float spawnPacingSeconds = 0.75f;
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        private readonly HashSet<BaseEnemy> activeEnemies = new HashSet<BaseEnemy>();
        private int currentWave;
        private Coroutine spawnRoutine;

        public event Action<ArenaState, string> StateChanged;

        public ArenaState State { get; private set; } = ArenaState.Preparing;
        public int CurrentWave => currentWave;
        public int ActiveEnemyCount => activeEnemies.Count;

        private void Start()
        {
            ResolveReferences();
            BeginArena();
        }

        private void Update()
        {
            if ((State == ArenaState.Victory || State == ArenaState.Failure) && Input.GetKeyDown(restartKey))
                RestartScene();
        }

        private void OnDisable()
        {
            if (player != null)
                player.OnDeath -= HandlePlayerDeath;

            foreach (BaseEnemy enemy in activeEnemies)
            {
                if (enemy != null)
                    enemy.Defeated -= HandleEnemyDefeated;
            }
            activeEnemies.Clear();
        }

        public void BeginArena()
        {
            if (player != null)
                player.OnDeath += HandlePlayerDeath;

            if (bossObject != null)
                bossObject.SetActive(false);

            currentWave = 1;
            RegisterInitialEnemies();

            if (activeEnemies.Count > 0)
                SetState(ArenaState.Wave, "Wave " + currentWave);
            else
                spawnRoutine = StartCoroutine(SpawnWaveRoutine());
        }

        public void RegisterEnemy(BaseEnemy enemy)
        {
            if (enemy == null || activeEnemies.Contains(enemy)) return;
            activeEnemies.Add(enemy);
            enemy.Defeated += HandleEnemyDefeated;
        }

        public void ConfigureForTests(BaseCharacter testPlayer, BaseEnemy[] testWaveEnemies, GameObject testBoss)
        {
            player = testPlayer;
            initialWaveEnemies = testWaveEnemies;
            bossObject = testBoss;
        }

        public void RestartScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                    player = playerObject.GetComponent<BaseCharacter>();
            }

            if (initialWaveEnemies == null || initialWaveEnemies.Length == 0)
                initialWaveEnemies = FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        private void RegisterInitialEnemies()
        {
            if (initialWaveEnemies == null) return;
            for (int i = 0; i < initialWaveEnemies.Length; i++)
            {
                BaseEnemy enemy = initialWaveEnemies[i];
                if (enemy == null) continue;
                if (bossObject != null && enemy.gameObject == bossObject) continue;
                RegisterEnemy(enemy);
            }
        }

        private IEnumerator SpawnWaveRoutine()
        {
            SetState(ArenaState.Wave, "Wave " + currentWave);
            int count = Mathf.Max(1, enemiesPerWave);
            var wait = new WaitForSeconds(Mathf.Max(0.05f, spawnPacingSeconds));
            for (int i = 0; i < count; i++)
            {
                SpawnWaveEnemy(i);
                yield return wait;
            }
            spawnRoutine = null;
        }

        private void SpawnWaveEnemy(int index)
        {
            if (waveEnemyPrefabs == null || waveEnemyPrefabs.Length == 0 || waveSpawnPoints == null || waveSpawnPoints.Length == 0)
                return;

            GameObject prefab = waveEnemyPrefabs[index % waveEnemyPrefabs.Length];
            Transform point = waveSpawnPoints[index % waveSpawnPoints.Length];
            if (prefab == null || point == null) return;

            GameObject enemyObject = Instantiate(prefab, point.position, point.rotation);
            RegisterEnemy(enemyObject.GetComponent<BaseEnemy>());
        }

        private void HandleEnemyDefeated(BaseEnemy enemy)
        {
            if (enemy != null)
                enemy.Defeated -= HandleEnemyDefeated;
            activeEnemies.Remove(enemy);

            if (activeEnemies.Count > 0 || State == ArenaState.Victory || State == ArenaState.Failure)
                return;

            if (State == ArenaState.Boss)
            {
                SetState(ArenaState.Victory, "Victory - press R");
                return;
            }

            if (currentWave < Mathf.Max(1, waveCount))
            {
                currentWave++;
                spawnRoutine = StartCoroutine(SpawnWaveRoutine());
                return;
            }

            ActivateBoss();
        }

        private void ActivateBoss()
        {
            if (bossObject == null)
            {
                SetState(ArenaState.Victory, "Victory - press R");
                return;
            }

            bossObject.SetActive(true);
            BaseEnemy boss = bossObject.GetComponent<BaseEnemy>();
            RegisterEnemy(boss);
            SetState(ArenaState.Boss, "Boss incoming");
        }

        private void HandlePlayerDeath(BaseCharacter character)
        {
            SetState(ArenaState.Failure, "Defeat - press R");
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);
        }

        private void SetState(ArenaState state, string message)
        {
            State = state;
            StateChanged?.Invoke(state, message);
        }
    }
}
