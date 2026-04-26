using System;
using System.Collections.Generic;
using System.IO;
using Game.Rhythm;
using UnityEngine;

namespace Game.Systems
{
    public class TelemetryManager : MonoBehaviour
    {
        private const string DirectoryName = "Telemetry";
        private const string TimestampFormat = "yyyyMMdd_HHmmss";

        public static TelemetryManager Instance { get; private set; }

        [SerializeField] private bool writeOnPlayerDeath = true;
        [SerializeField] private int timingSampleCapacity = 512;

        private readonly List<float> timingOffsets = new List<float>(512);
        private readonly List<int> completedComboLengths = new List<int>(64);

        private float runStartTime;
        private float firstEnemyKillTime = -1f;
        private float lastEnemyKillTime = -1f;
        private float playerDeathTime = -1f;
        private int currentComboLength;
        private int scoreAtRunStart;
        private string lastWrittenPath;

        public TelemetrySnapshot Snapshot { get; private set; } = new TelemetrySnapshot();
        public string LastWrittenPath => lastWrittenPath;
        public int CurrentComboLength => currentComboLength;
        public float AverageTimingOffset => Snapshot.averageTimingOffset;
        public float ScorePerMinute
        {
            get
            {
                UpdateDerivedMetrics();
                return Snapshot.scorePerMinute;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            timingOffsets.Capacity = Mathf.Max(timingOffsets.Capacity, timingSampleCapacity);
            ResetRun();
        }

        private void OnEnable()
        {
            Core.BaseEnemy.EnemyDefeatedGlobal += HandleEnemyDefeated;
        }

        private void OnDisable()
        {
            Core.BaseEnemy.EnemyDefeatedGlobal -= HandleEnemyDefeated;
            if (Instance == this)
                Instance = null;
        }

        public void ResetRun()
        {
            runStartTime = Time.time;
            firstEnemyKillTime = -1f;
            lastEnemyKillTime = -1f;
            playerDeathTime = -1f;
            currentComboLength = 0;
            scoreAtRunStart = Core.ScoreSystem.Instance != null ? Core.ScoreSystem.Instance.Score : 0;
            lastWrittenPath = string.Empty;
            timingOffsets.Clear();
            completedComboLengths.Clear();
            Snapshot = new TelemetrySnapshot
            {
                runStartedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        public static void ReportInputJudgement(RhythmGrade grade, float signedTimingOffsetSeconds)
        {
            if (Instance != null)
                Instance.RecordInputJudgement(grade, signedTimingOffsetSeconds);
        }

        public static void ReportHit(RhythmGrade grade)
        {
            if (Instance != null)
                Instance.RecordHit(grade);
        }

        public static void ReportDodge(RhythmGrade grade)
        {
            if (Instance != null)
                Instance.RecordDodge(grade);
        }

        public static void ReportComboResolved(int comboLength)
        {
            if (Instance != null)
                Instance.RecordComboResolved(comboLength);
        }

        public static void ReportComboReset(int comboLength)
        {
            if (Instance != null)
                Instance.RecordComboReset(comboLength);
        }

        public static void ReportScore(int totalScore)
        {
            if (Instance != null)
                Instance.RecordScore(totalScore);
        }

        public static void ReportPlayerDeath()
        {
            if (Instance != null)
                Instance.RecordPlayerDeath();
        }

        public string WriteRunSummary()
        {
            try
            {
                UpdateDerivedMetrics();
                Snapshot.timingOffsets = timingOffsets.ToArray();
                Snapshot.runEndedAtUtc = DateTime.UtcNow.ToString("o");
                string directory = Path.Combine(Application.persistentDataPath, DirectoryName);
                Directory.CreateDirectory(directory);
                string timestamp = DateTime.UtcNow.ToString(TimestampFormat);
                string path = Path.Combine(directory, "run_" + timestamp + ".json");
                File.WriteAllText(path, JsonUtility.ToJson(Snapshot, true));
                lastWrittenPath = path;
                return path;
            }
            catch (Exception ex)
            {
                lastWrittenPath = string.Empty;
                Debug.LogWarning("Telemetry summary write failed: " + ex.Message);
                return string.Empty;
            }
        }

        private void RecordInputJudgement(RhythmGrade grade, float signedTimingOffsetSeconds)
        {
            if (timingOffsets.Count < Mathf.Max(1, timingSampleCapacity))
                timingOffsets.Add(signedTimingOffsetSeconds);

            UpdateAverageTimingOffset();
        }

        private void RecordHit(RhythmGrade grade)
        {
            switch (grade)
            {
                case RhythmGrade.Perfect:
                    Snapshot.perfectHitCount++;
                    break;
                case RhythmGrade.Good:
                    Snapshot.goodHitCount++;
                    break;
                default:
                    Snapshot.missHitCount++;
                    break;
            }
        }

        private void RecordDodge(RhythmGrade grade)
        {
            switch (grade)
            {
                case RhythmGrade.Perfect:
                    Snapshot.perfectDodgeCount++;
                    break;
                case RhythmGrade.Good:
                    Snapshot.goodDodgeCount++;
                    break;
                default:
                    Snapshot.missDodgeCount++;
                    break;
            }
        }

        private void RecordComboResolved(int comboLength)
        {
            currentComboLength = Mathf.Max(0, comboLength);
            Snapshot.maxCombo = Mathf.Max(Snapshot.maxCombo, currentComboLength);
        }

        private void RecordComboReset(int comboLength)
        {
            int safeLength = Mathf.Max(0, comboLength);
            if (safeLength > 0)
                completedComboLengths.Add(safeLength);
            currentComboLength = 0;
            UpdateAverageComboLength();
        }

        private void RecordScore(int totalScore)
        {
            Snapshot.totalScore = Mathf.Max(0, totalScore - scoreAtRunStart);
            UpdateDerivedMetrics();
        }

        private void RecordPlayerDeath()
        {
            if (playerDeathTime < 0f)
                playerDeathTime = Time.time;

            UpdateDerivedMetrics();
            if (writeOnPlayerDeath)
                WriteRunSummary();
        }

        private void HandleEnemyDefeated(Core.BaseEnemy enemy)
        {
            if (firstEnemyKillTime < 0f)
                firstEnemyKillTime = Time.time;
            lastEnemyKillTime = Time.time;
            Snapshot.enemyKillCount++;
            UpdateDerivedMetrics();
        }

        private void UpdateAverageTimingOffset()
        {
            float total = 0f;
            for (int i = 0; i < timingOffsets.Count; i++)
                total += timingOffsets[i];

            Snapshot.averageTimingOffset = timingOffsets.Count > 0 ? total / timingOffsets.Count : 0f;
        }

        private void UpdateAverageComboLength()
        {
            if (completedComboLengths.Count == 0)
            {
                Snapshot.averageComboLength = currentComboLength;
                return;
            }

            int total = 0;
            for (int i = 0; i < completedComboLengths.Count; i++)
                total += completedComboLengths[i];
            Snapshot.averageComboLength = (float)total / completedComboLengths.Count;
        }

        private void UpdateDerivedMetrics()
        {
            float elapsed = Mathf.Max(0f, (playerDeathTime >= 0f ? playerDeathTime : Time.time) - runStartTime);
            Snapshot.totalSurvivalTime = elapsed;
            Snapshot.timeToFirstDeath = playerDeathTime >= 0f ? Mathf.Max(0f, playerDeathTime - runStartTime) : 0f;
            Snapshot.scorePerMinute = elapsed > 0.01f ? Snapshot.totalScore / (elapsed / 60f) : 0f;
            Snapshot.timePerKill = Snapshot.enemyKillCount > 0 && lastEnemyKillTime >= 0f
                ? Mathf.Max(0f, (lastEnemyKillTime - runStartTime) / Snapshot.enemyKillCount)
                : 0f;
            UpdateAverageComboLength();
        }
    }

    [Serializable]
    public class TelemetrySnapshot
    {
        public string runStartedAtUtc;
        public string runEndedAtUtc;
        public int perfectHitCount;
        public int goodHitCount;
        public int missHitCount;
        public int perfectDodgeCount;
        public int goodDodgeCount;
        public int missDodgeCount;
        public float[] timingOffsets = Array.Empty<float>();
        public float averageTimingOffset;
        public float timeToFirstDeath;
        public float totalSurvivalTime;
        public int enemyKillCount;
        public float timePerKill;
        public int maxCombo;
        public float averageComboLength;
        public int totalScore;
        public float scorePerMinute;
    }
}
