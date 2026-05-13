using System;
using System.Collections.Generic;
using System.IO;
using Game.Rhythm;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class TelemetryManager : MonoBehaviour
    {
        private const string DirectoryName = "Telemetry";
        private const string TimestampFormat = "yyyyMMdd_HHmmss";
        private const int SchemaVersion = 3;

        public static TelemetryManager Instance { get; private set; }

        [SerializeField] private bool writeOnPlayerDeath = true;
        [SerializeField] private int timingSampleCapacity = 512;
        [SerializeField] private string batchLabel;
        [SerializeField] private string runNotes;

        private readonly List<float> timingOffsets = new List<float>(512);
        private readonly List<int> completedComboLengths = new List<int>(64);

        private float runStartTime;
        private float firstEnemyKillTime = -1f;
        private float lastEnemyKillTime = -1f;
        private float playerDeathTime = -1f;
        private int currentComboLength;
        private int scoreAtRunStart;
        private string lastWrittenPath;
        private string telemetryRootOverride;
        private bool suppressWriteWarningsForTests;
        private bool runEnded;

        public TelemetrySnapshot Snapshot { get; private set; } = new TelemetrySnapshot();
        public string LastWrittenPath => lastWrittenPath;
        public string BatchLabel => batchLabel;
        public string RunNotes => runNotes;
        public string RunEndReason => Snapshot != null ? Snapshot.runEndReason : string.Empty;
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
            runEnded = false;
            timingOffsets.Clear();
            completedComboLengths.Clear();
            Snapshot = new TelemetrySnapshot
            {
                schemaVersion = SchemaVersion,
                runStartedAtUtc = DateTime.UtcNow.ToString("o"),
                sceneName = SceneManager.GetActiveScene().name,
                playerClass = ResolvePlayerClassName(),
                batchLabel = SanitizeBatchLabel(batchLabel),
                runNotes = runNotes ?? string.Empty,
                appVersion = Application.version,
                unityVersion = Application.unityVersion
            };
        }

        public void StartNewRun()
        {
            ResetRun();
        }

        public void ClearCurrentRunData()
        {
            ResetRun();
        }

        public void SetBatchLabel(string value)
        {
            batchLabel = SanitizeBatchLabel(value);
            if (Snapshot != null)
                Snapshot.batchLabel = batchLabel;
        }

        public void SetRunNotes(string value)
        {
            runNotes = value ?? string.Empty;
            if (Snapshot != null)
                Snapshot.runNotes = runNotes;
        }

        public string GetTelemetryDirectory()
        {
            return Path.Combine(GetTelemetryRoot(), DirectoryName);
        }

        public string GetCurrentRunDirectory()
        {
            string root = GetTelemetryDirectory();
            string safeBatch = SanitizeBatchLabel(batchLabel);
            return string.IsNullOrEmpty(safeBatch) ? root : Path.Combine(root, safeBatch);
        }

        public void ConfigureTelemetryRootForTests(string rootPath)
        {
            telemetryRootOverride = rootPath;
        }

        public void ClearTelemetryRootOverrideForTests()
        {
            telemetryRootOverride = null;
        }

        public void SuppressWriteWarningsForTests(bool suppress)
        {
            suppressWriteWarningsForTests = suppress;
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

        public static void ReportSessionComplete()
        {
            if (Instance != null)
                Instance.EndRun("session_complete", true);
        }

        public string EndRun(string endReason, bool writeSummary)
        {
            if (!string.IsNullOrWhiteSpace(endReason))
                Snapshot.runEndReason = SanitizeRunEndReason(endReason);

            if (!runEnded)
            {
                runEnded = true;
                Snapshot.runEnded = true;
            }

            if (!writeSummary)
                return string.Empty;

            if (!string.IsNullOrEmpty(lastWrittenPath))
                return lastWrittenPath;

            return WriteRunSummary();
        }

        public string WriteRunSummary()
        {
            try
            {
                if (runEnded && !string.IsNullOrEmpty(lastWrittenPath))
                    return lastWrittenPath;

                if (!runEnded)
                {
                    runEnded = true;
                    Snapshot.runEnded = true;
                    if (string.IsNullOrEmpty(Snapshot.runEndReason))
                        Snapshot.runEndReason = "manual_write";
                }

                UpdateDerivedMetrics();
                Snapshot.sceneName = SceneManager.GetActiveScene().name;
                Snapshot.playerClass = ResolvePlayerClassName();
                Snapshot.batchLabel = SanitizeBatchLabel(batchLabel);
                Snapshot.runNotes = runNotes ?? string.Empty;
                Snapshot.timingOffsets = timingOffsets.ToArray();
                Snapshot.runEndedAtUtc = DateTime.UtcNow.ToString("o");
                string directory = GetCurrentRunDirectory();
                Directory.CreateDirectory(directory);
                string timestamp = DateTime.UtcNow.ToString(TimestampFormat);
                string fileName = string.IsNullOrEmpty(Snapshot.batchLabel)
                    ? "run_" + timestamp + ".json"
                    : "run_" + timestamp + "_" + Snapshot.batchLabel + ".json";
                string path = GetUniquePath(Path.Combine(directory, fileName));
                File.WriteAllText(path, JsonUtility.ToJson(Snapshot, true));
                lastWrittenPath = path;
                return path;
            }
            catch (Exception ex)
            {
                lastWrittenPath = string.Empty;
                if (!suppressWriteWarningsForTests)
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
            EndRun("defeat", writeOnPlayerDeath);
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
            Snapshot.runDurationSeconds = elapsed;
            Snapshot.totalSurvivalTime = elapsed;
            Snapshot.timeToFirstDeath = playerDeathTime >= 0f ? Mathf.Max(0f, playerDeathTime - runStartTime) : 0f;
            Snapshot.scorePerMinute = elapsed > 0.01f ? Snapshot.totalScore / (elapsed / 60f) : 0f;
            Snapshot.timePerKill = Snapshot.enemyKillCount > 0 && lastEnemyKillTime >= 0f
                ? Mathf.Max(0f, (lastEnemyKillTime - runStartTime) / Snapshot.enemyKillCount)
                : 0f;
            UpdateAverageComboLength();
        }

        private static string ResolvePlayerClassName()
        {
            Core.PlayerManager manager = Core.PlayerManager.Instance;
            if (manager == null || manager.Player == null)
                return string.Empty;

            Core.BaseCharacter character = manager.Player.GetComponent<Core.BaseCharacter>();
            return character != null ? character.CharacterName : manager.Player.name;
        }

        private string GetTelemetryRoot()
        {
            return string.IsNullOrEmpty(telemetryRootOverride) ? Application.persistentDataPath : telemetryRootOverride;
        }

        private static string GetUniquePath(string desiredPath)
        {
            if (!File.Exists(desiredPath))
                return desiredPath;

            string directory = Path.GetDirectoryName(desiredPath);
            string name = Path.GetFileNameWithoutExtension(desiredPath);
            string extension = Path.GetExtension(desiredPath);
            for (int i = 1; i < 1000; i++)
            {
                string candidate = Path.Combine(directory, name + "_" + i.ToString("000") + extension);
                if (!File.Exists(candidate))
                    return candidate;
            }

            return Path.Combine(directory, name + "_" + Guid.NewGuid().ToString("N") + extension);
        }

        public static string SanitizeBatchLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string trimmed = value.Trim();
            var chars = new char[trimmed.Length];
            int count = 0;
            bool previousSeparator = false;
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                bool safe = char.IsLetterOrDigit(c) || c == '-' || c == '_';
                char next = safe ? c : '_';
                if (next == '_')
                {
                    if (previousSeparator)
                        continue;
                    previousSeparator = true;
                }
                else
                {
                    previousSeparator = false;
                }

                chars[count++] = next;
            }

            return count > 0 ? new string(chars, 0, count).Trim('_') : string.Empty;
        }

        private static string SanitizeRunEndReason(string value)
        {
            string sanitized = SanitizeBatchLabel(value);
            return string.IsNullOrEmpty(sanitized) ? "unknown" : sanitized.ToLowerInvariant();
        }
    }

    [Serializable]
    public class TelemetrySnapshot
    {
        public int schemaVersion;
        public string runStartedAtUtc;
        public string runEndedAtUtc;
        public float runDurationSeconds;
        public string sceneName;
        public string playerClass;
        public string batchLabel;
        public string runNotes;
        public bool runEnded;
        public string runEndReason;
        public string appVersion;
        public string unityVersion;
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
