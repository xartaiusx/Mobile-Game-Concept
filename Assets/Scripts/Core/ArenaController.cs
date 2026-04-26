using System;
using System.Collections;
using System.Collections.Generic;
using Game.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public enum ArenaState
    {
        Preparing,
        Wave,
        WaveCleared,
        Boss,
        Elite,
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
        [SerializeField] private float nextWaveDelaySeconds = 2f;
        [SerializeField] private KeyCode restartKey = KeyCode.R;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private PickupSpawner pickupSpawner;
        [SerializeField] private DifficultyScaler difficultyScaler;

        private readonly HashSet<BaseEnemy> activeEnemies = new HashSet<BaseEnemy>();
        private int currentWave;
        private Coroutine spawnRoutine;
        private GameObject bossTemplate;
        private string lastStateMessage = "Preparing";

        public event Action<ArenaState, string> StateChanged;

        public ArenaState State { get; private set; } = ArenaState.Preparing;
        public int CurrentWave => currentWave;
        public int ActiveEnemyCount => activeEnemies.Count;
        public string LastStateMessage => lastStateMessage;
        public bool IsBossOrEliteWave => difficultyScaler != null && difficultyScaler.IsBossWave(currentWave);

        private void Start()
        {
            ResolveReferences();
            BeginArena();
        }

        private void Update()
        {
            if (Input.GetKeyDown(restartKey))
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

            TelemetryManager.Instance?.ResetRun();
            scoreSystem?.ResetScore();

            if (bossObject != null)
            {
                bossTemplate = bossObject;
                bossObject.SetActive(false);
            }

            currentWave = 1;
            RegisterInitialEnemies();
            ApplyWaveDifficultyToActiveEnemies(false);

            if (activeEnemies.Count > 0)
                StartCurrentWaveState();
            else
                BeginCurrentWave();
        }

        public void RegisterEnemy(BaseEnemy enemy)
        {
            if (enemy == null || activeEnemies.Contains(enemy)) return;
            activeEnemies.Add(enemy);
            enemy.Defeated += HandleEnemyDefeated;
        }

        public void ReplacePlayer(BaseCharacter newPlayer)
        {
            if (player == newPlayer) return;

            if (player != null)
                player.OnDeath -= HandlePlayerDeath;

            player = newPlayer;

            if (player != null && State != ArenaState.Failure && State != ArenaState.Victory)
                player.OnDeath += HandlePlayerDeath;
        }

        public void ConfigureForTests(BaseCharacter testPlayer, BaseEnemy[] testWaveEnemies, GameObject testBoss, DifficultyScaler testDifficultyScaler = null)
        {
            player = testPlayer;
            initialWaveEnemies = testWaveEnemies;
            bossObject = testBoss;
            bossTemplate = testBoss;
            difficultyScaler = testDifficultyScaler;
        }

        public void CompleteActiveWaveForTests()
        {
            CompleteWave();
        }

        public void BeginNextWaveForTests()
        {
            BeginNextWave();
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
                initialWaveEnemies = FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude);

            if (scoreSystem == null)
                scoreSystem = FindAnyObjectByType<ScoreSystem>();
            if (pickupSpawner == null)
                pickupSpawner = FindAnyObjectByType<PickupSpawner>();
            if (difficultyScaler == null)
                difficultyScaler = GetComponent<DifficultyScaler>() ?? gameObject.AddComponent<DifficultyScaler>();
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

        private void BeginCurrentWave()
        {
            if (difficultyScaler != null && difficultyScaler.IsBossWave(currentWave))
            {
                ActivateBossOrElite();
                return;
            }

            StartCurrentWaveState();
            spawnRoutine = StartCoroutine(SpawnWaveRoutine());
        }

        private IEnumerator SpawnWaveRoutine()
        {
            StartCurrentWaveState();
            int count = difficultyScaler != null ? difficultyScaler.GetEnemyCount(currentWave) : Mathf.Max(1, enemiesPerWave);
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
            BaseEnemy enemy = enemyObject.GetComponent<BaseEnemy>();
            ApplyDifficulty(enemy, false);
            RegisterEnemy(enemy);
        }

        private void HandleEnemyDefeated(BaseEnemy enemy)
        {
            if (enemy != null)
                enemy.Defeated -= HandleEnemyDefeated;
            activeEnemies.Remove(enemy);

            if (activeEnemies.Count > 0 || State == ArenaState.Failure)
                return;

            CompleteWave();
        }

        private void CompleteWave()
        {
            bool bossWave = State == ArenaState.Boss || State == ArenaState.Elite || IsBossOrEliteWave;
            if (bossWave)
            {
                scoreSystem?.AddBossClearBonus(currentWave);
                pickupSpawner?.SpawnVictoryBonus(player != null ? player.transform.position : transform.position);
            }
            else
            {
                scoreSystem?.AddWaveClearBonus(currentWave);
            }

            SetState(ArenaState.WaveCleared, "Wave " + currentWave + " cleared");
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(NextWaveRoutine());
        }

        private IEnumerator NextWaveRoutine()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, nextWaveDelaySeconds));
            BeginNextWave();
        }

        private void BeginNextWave()
        {
            currentWave++;
            BeginCurrentWave();
        }

        private void ActivateBossOrElite()
        {
            GameObject template = bossTemplate != null ? bossTemplate : bossObject;
            if (template == null)
            {
                StartCurrentWaveState();
                spawnRoutine = StartCoroutine(SpawnWaveRoutine());
                return;
            }

            GameObject bossInstance = template.scene.IsValid()
                ? Instantiate(template, template.transform.position, template.transform.rotation)
                : Instantiate(template, ResolveSpawnPosition(0), Quaternion.identity);
            bossInstance.name = currentWave % Mathf.Max(1, difficultyScaler != null ? difficultyScaler.BossEveryWaves * 2 : 10) == 0 ? "EndlessBoss" : "EndlessElite";
            bossInstance.SetActive(true);
            BaseEnemy boss = bossInstance.GetComponent<BaseEnemy>();
            ApplyDifficulty(boss, true);
            RegisterEnemy(boss);
            SetState(ArenaState.Boss, "Boss/Elite warning - Wave " + currentWave);
        }

        private void StartCurrentWaveState()
        {
            ApplyRhythmAndPickupScaling();
            string message = IsBossOrEliteWave ? "Boss/Elite warning - Wave " + currentWave : "Wave " + currentWave;
            SetState(IsBossOrEliteWave ? ArenaState.Boss : ArenaState.Wave, message);
        }

        private Vector3 ResolveSpawnPosition(int index)
        {
            if (waveSpawnPoints != null && waveSpawnPoints.Length > 0 && waveSpawnPoints[index % waveSpawnPoints.Length] != null)
                return waveSpawnPoints[index % waveSpawnPoints.Length].position;
            return transform.position + Vector3.forward * 6f;
        }

        private void ApplyWaveDifficultyToActiveEnemies(bool bossOrElite)
        {
            foreach (BaseEnemy enemy in activeEnemies)
                ApplyDifficulty(enemy, bossOrElite);
            ApplyRhythmAndPickupScaling();
        }

        private void ApplyDifficulty(BaseEnemy enemy, bool bossOrElite)
        {
            if (enemy == null || difficultyScaler == null) return;
            DifficultySnapshot snapshot = difficultyScaler.Evaluate(currentWave, bossOrElite);
            enemy.ApplyEndlessDifficulty(snapshot.healthMultiplier, snapshot.damageMultiplier, snapshot.moveSpeedMultiplier, snapshot.attackCooldownMultiplier);
        }

        private void ApplyRhythmAndPickupScaling()
        {
            if (difficultyScaler == null) return;
            DifficultySnapshot snapshot = difficultyScaler.Evaluate(currentWave, IsBossOrEliteWave);
            Game.Rhythm.BeatClock.Instance?.SetLevelSpeedMultiplier(snapshot.beatSpeedMultiplier);
            pickupSpawner?.SetGenerosityMultiplier(snapshot.pickupGenerosityMultiplier);
        }

        private void HandlePlayerDeath(BaseCharacter character)
        {
            SetState(ArenaState.Failure, "Defeat - press R to restart");
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);
        }

        private void SetState(ArenaState state, string message)
        {
            State = state;
            lastStateMessage = message;
            StateChanged?.Invoke(state, message);
        }
    }
}
