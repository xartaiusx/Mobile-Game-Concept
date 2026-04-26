using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Systems
{
    public static class TelemetryAnalysis
    {
        public const float ShortSessionThresholdSeconds = 20f;

        private const float PerfectHitRateMin = 0.20f;
        private const float PerfectHitRateMax = 0.40f;
        private const float MissHitRateMax = 0.25f;
        private const float PerfectDodgeRateMin = 0.10f;
        private const float PerfectDodgeRateMax = 0.25f;
        private const float MissDodgeRateMax = 0.35f;
        private const float TimingBiasThresholdSeconds = 0.05f;
        private const float AverageComboMin = 3f;
        private const float AverageComboMax = 6f;
        private const float SurvivalTargetMinSeconds = 30f;
        private const float SurvivalTargetMaxSeconds = 90f;

        public static TelemetryParseResult ParseJson(string json, string sourcePath = "")
        {
            var result = new TelemetryParseResult { sourcePath = sourcePath };
            if (string.IsNullOrWhiteSpace(json))
            {
                result.errorMessage = "Telemetry JSON is empty.";
                return result;
            }

            try
            {
                result.snapshot = JsonUtility.FromJson<TelemetrySnapshot>(json);
                if (result.snapshot == null)
                    result.errorMessage = "Telemetry JSON did not contain a snapshot object.";
            }
            catch (Exception ex)
            {
                result.errorMessage = ex.Message;
            }

            return result;
        }

        public static TelemetryAnalysisSummary AnalyzeDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return AnalyzeFiles(Array.Empty<string>());

            return AnalyzeFiles(Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly));
        }

        public static TelemetryAnalysisSummary AnalyzeFiles(IEnumerable<string> filePaths)
        {
            var summary = new TelemetryAnalysisSummary();
            foreach (string path in filePaths)
            {
                summary.fileCount++;
                try
                {
                    TelemetryParseResult parsed = ParseJson(File.ReadAllText(path), path);
                    if (!parsed.IsValid)
                    {
                        summary.parseErrors.Add(new TelemetryParseError(path, parsed.errorMessage));
                        continue;
                    }

                    summary.runs.Add(CreateRunMetrics(parsed.snapshot, path));
                }
                catch (Exception ex)
                {
                    summary.parseErrors.Add(new TelemetryParseError(path, ex.Message));
                }
            }

            CompleteSummary(summary);
            return summary;
        }

        public static TelemetryAnalysisSummary AnalyzeSnapshots(IEnumerable<TelemetrySnapshot> snapshots)
        {
            var summary = new TelemetryAnalysisSummary();
            foreach (TelemetrySnapshot snapshot in snapshots)
            {
                summary.fileCount++;
                if (snapshot == null)
                {
                    summary.parseErrors.Add(new TelemetryParseError(string.Empty, "Snapshot is null."));
                    continue;
                }

                summary.runs.Add(CreateRunMetrics(snapshot, string.Empty));
            }

            CompleteSummary(summary);
            return summary;
        }

        public static TelemetryRunMetrics CreateRunMetrics(TelemetrySnapshot snapshot, string sourcePath = "")
        {
            var metrics = new TelemetryRunMetrics
            {
                sourcePath = sourcePath,
                snapshot = snapshot,
                runDurationSeconds = ResolveRunDuration(snapshot),
                averageTimingOffset = snapshot.averageTimingOffset,
                scorePerMinute = snapshot.scorePerMinute,
                maxCombo = snapshot.maxCombo,
                averageComboLength = snapshot.averageComboLength,
                totalScore = snapshot.totalScore,
                enemyKillCount = snapshot.enemyKillCount
            };

            metrics.hitTotal = snapshot.perfectHitCount + snapshot.goodHitCount + snapshot.missHitCount;
            metrics.perfectHitRate = Rate(snapshot.perfectHitCount, metrics.hitTotal);
            metrics.goodHitRate = Rate(snapshot.goodHitCount, metrics.hitTotal);
            metrics.missHitRate = Rate(snapshot.missHitCount, metrics.hitTotal);

            metrics.dodgeTotal = snapshot.perfectDodgeCount + snapshot.goodDodgeCount + snapshot.missDodgeCount;
            metrics.perfectDodgeRate = Rate(snapshot.perfectDodgeCount, metrics.dodgeTotal);
            metrics.goodDodgeRate = Rate(snapshot.goodDodgeCount, metrics.dodgeTotal);
            metrics.missDodgeRate = Rate(snapshot.missDodgeCount, metrics.dodgeTotal);

            metrics.isShortSession = metrics.runDurationSeconds > 0f && metrics.runDurationSeconds < ShortSessionThresholdSeconds;
            metrics.enemyKillsPerMinute = metrics.runDurationSeconds > 0.01f
                ? snapshot.enemyKillCount / (metrics.runDurationSeconds / 60f)
                : 0f;

            float[] offsets = snapshot.timingOffsets ?? Array.Empty<float>();
            int early = 0;
            int late = 0;
            for (int i = 0; i < offsets.Length; i++)
            {
                if (offsets[i] < 0f)
                    early++;
                else if (offsets[i] > 0f)
                    late++;
            }

            metrics.timingSampleCount = offsets.Length;
            metrics.earlyInputRate = Rate(early, offsets.Length);
            metrics.lateInputRate = Rate(late, offsets.Length);
            return metrics;
        }

        public static string ToHumanReadableSummary(TelemetryAnalysisSummary summary)
        {
            if (summary == null)
                return "Telemetry analysis unavailable.";

            var builder = new StringBuilder(1024);
            builder.AppendLine("Telemetry Analysis Summary");
            builder.AppendLine("Files: " + summary.fileCount + "  Valid runs: " + summary.validRunCount + "  Short/smoke: " + summary.shortSessionCount + "  Parse errors: " + summary.parseErrors.Count);
            builder.AppendLine("Average run duration: " + Seconds(summary.averageRunDurationSeconds) + "  Normal run duration: " + Seconds(summary.averageNormalRunDurationSeconds));
            builder.AppendLine("Hits P/G/M: " + Percent(summary.averagePerfectHitRate) + " / " + Percent(summary.averageGoodHitRate) + " / " + Percent(summary.averageMissHitRate));
            builder.AppendLine("Dodges P/G/M: " + Percent(summary.averagePerfectDodgeRate) + " / " + Percent(summary.averageGoodDodgeRate) + " / " + Percent(summary.averageMissDodgeRate));
            builder.AppendLine("Timing avg: " + SignedSeconds(summary.averageTimingOffset) + "  Early/Late: " + Percent(summary.averageEarlyInputRate) + " / " + Percent(summary.averageLateInputRate));
            builder.AppendLine("Score/min normal: " + summary.averageScorePerMinuteExcludingShort.ToString("0.0", CultureInfo.InvariantCulture) + "  Kills/min normal: " + summary.averageEnemyKillsPerMinuteExcludingShort.ToString("0.0", CultureInfo.InvariantCulture));
            builder.AppendLine("Combo avg/max: " + summary.averageComboLength.ToString("0.0", CultureInfo.InvariantCulture) + " / " + summary.averageMaxCombo.ToString("0.0", CultureInfo.InvariantCulture));

            if (summary.warnings.Count > 0)
            {
                builder.AppendLine("Warnings:");
                for (int i = 0; i < summary.warnings.Count; i++)
                    builder.AppendLine("- " + summary.warnings[i]);
            }
            else
            {
                builder.AppendLine("Warnings: none.");
            }

            return builder.ToString();
        }

        private static void CompleteSummary(TelemetryAnalysisSummary summary)
        {
            summary.validRunCount = summary.runs.Count;
            if (summary.validRunCount == 0)
            {
                summary.warnings.Add("No valid telemetry runs found. Run 5-10 Warrior sessions before tuning.");
                return;
            }

            int normalCount = 0;
            for (int i = 0; i < summary.runs.Count; i++)
            {
                TelemetryRunMetrics run = summary.runs[i];
                if (run.isShortSession)
                    summary.shortSessionCount++;
                else
                    normalCount++;

                summary.averagePerfectHitRate += run.perfectHitRate;
                summary.averageGoodHitRate += run.goodHitRate;
                summary.averageMissHitRate += run.missHitRate;
                summary.averagePerfectDodgeRate += run.perfectDodgeRate;
                summary.averageGoodDodgeRate += run.goodDodgeRate;
                summary.averageMissDodgeRate += run.missDodgeRate;
                summary.averageTimingOffset += run.averageTimingOffset;
                summary.averageEarlyInputRate += run.earlyInputRate;
                summary.averageLateInputRate += run.lateInputRate;
                summary.averageMaxCombo += run.maxCombo;
                summary.averageComboLength += run.averageComboLength;
                summary.averageRunDurationSeconds += run.runDurationSeconds;

                if (!run.isShortSession)
                {
                    summary.averageScorePerMinuteExcludingShort += run.scorePerMinute;
                    summary.averageEnemyKillsPerMinuteExcludingShort += run.enemyKillsPerMinute;
                    summary.averageNormalRunDurationSeconds += run.runDurationSeconds;
                }
            }

            float valid = summary.validRunCount;
            summary.averagePerfectHitRate /= valid;
            summary.averageGoodHitRate /= valid;
            summary.averageMissHitRate /= valid;
            summary.averagePerfectDodgeRate /= valid;
            summary.averageGoodDodgeRate /= valid;
            summary.averageMissDodgeRate /= valid;
            summary.averageTimingOffset /= valid;
            summary.averageEarlyInputRate /= valid;
            summary.averageLateInputRate /= valid;
            summary.averageMaxCombo /= valid;
            summary.averageComboLength /= valid;
            summary.averageRunDurationSeconds /= valid;

            if (normalCount > 0)
            {
                summary.averageScorePerMinuteExcludingShort /= normalCount;
                summary.averageEnemyKillsPerMinuteExcludingShort /= normalCount;
                summary.averageNormalRunDurationSeconds /= normalCount;
            }

            AddWarnings(summary, normalCount);
        }

        private static void AddWarnings(TelemetryAnalysisSummary summary, int normalCount)
        {
            if (summary.shortSessionCount > 0)
                summary.warnings.Add(summary.shortSessionCount + " short/smoke session(s) excluded from score-per-minute and kill-rate tuning.");

            if (normalCount == 0)
            {
                summary.warnings.Add("Only short sessions were found. Treat score/minute as smoke-test distorted and collect normal runs before tuning.");
                return;
            }

            if (summary.averagePerfectHitRate < PerfectHitRateMin)
                summary.warnings.Add("Perfect hit rate is low: consider clarifying beat feedback or slightly widening perfectWindow.");
            else if (summary.averagePerfectHitRate > PerfectHitRateMax)
                summary.warnings.Add("Perfect hit rate is high: consider tightening perfectWindow after confirming readability feels fair.");

            if (summary.averageMissHitRate > MissHitRateMax)
                summary.warnings.Add("Miss rate is high: consider widening goodWindow or improving enemy telegraphs.");

            if (summary.averagePerfectDodgeRate < PerfectDodgeRateMin)
                summary.warnings.Add("Perfect dodge rate is too low: consider widening dodge timing or improving telegraph readability.");
            else if (summary.averagePerfectDodgeRate > PerfectDodgeRateMax)
                summary.warnings.Add("Perfect dodge rate is high: consider tightening dodge timing after validating mobile input latency.");

            if (summary.averageMissDodgeRate > MissDodgeRateMax)
                summary.warnings.Add("Miss dodge rate is high: consider improving defensive telegraph readability or dodge input forgiveness.");

            if (summary.averageTimingOffset < -TimingBiasThresholdSeconds)
                summary.warnings.Add("Average input is early: consider reducing early bias or shifting beat bar alignment.");
            else if (summary.averageTimingOffset > TimingBiasThresholdSeconds)
                summary.warnings.Add("Average input is late: consider increasing input lead feedback or shifting beat bar alignment.");

            if (summary.averageComboLength < AverageComboMin)
                summary.warnings.Add("Average combo length is low: consider softening miss penalty or improving hit feedback.");
            else if (summary.averageComboLength > AverageComboMax)
                summary.warnings.Add("Average combo length is high: consider adding more pressure before raising rewards.");

            if (summary.averageMaxCombo < AverageComboMin)
                summary.warnings.Add("Max combo is consistently below 3: inspect attack cadence, hit confirmation, and miss recovery.");

            if (summary.averageNormalRunDurationSeconds < SurvivalTargetMinSeconds)
                summary.warnings.Add("Normal run survival is short: consider lowering early wave pressure or improving defensive readability.");
            else if (summary.averageNormalRunDurationSeconds > SurvivalTargetMaxSeconds)
                summary.warnings.Add("Normal run survival is long: consider increasing wave pressure after confirming combat remains readable.");
        }

        private static float ResolveRunDuration(TelemetrySnapshot snapshot)
        {
            if (snapshot.runDurationSeconds > 0f)
                return snapshot.runDurationSeconds;
            if (snapshot.totalSurvivalTime > 0f)
                return snapshot.totalSurvivalTime;
            if (snapshot.timeToFirstDeath > 0f)
                return snapshot.timeToFirstDeath;

            if (DateTime.TryParse(snapshot.runStartedAtUtc, null, DateTimeStyles.RoundtripKind, out DateTime start)
                && DateTime.TryParse(snapshot.runEndedAtUtc, null, DateTimeStyles.RoundtripKind, out DateTime end)
                && end > start)
                return (float)(end - start).TotalSeconds;

            return 0f;
        }

        private static float Rate(int count, int total)
        {
            return total > 0 ? Mathf.Clamp01((float)count / total) : 0f;
        }

        private static string Percent(float value)
        {
            return (value * 100f).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        }

        private static string Seconds(float value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        private static string SignedSeconds(float value)
        {
            return (value >= 0f ? "+" : string.Empty) + value.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }
    }

    public sealed class TelemetryParseResult
    {
        public string sourcePath;
        public TelemetrySnapshot snapshot;
        public string errorMessage;
        public bool IsValid => snapshot != null && string.IsNullOrEmpty(errorMessage);
    }

    public sealed class TelemetryParseError
    {
        public string sourcePath;
        public string message;

        public TelemetryParseError(string sourcePath, string message)
        {
            this.sourcePath = sourcePath;
            this.message = message;
        }
    }

    public sealed class TelemetryRunMetrics
    {
        public string sourcePath;
        public TelemetrySnapshot snapshot;
        public float runDurationSeconds;
        public bool isShortSession;
        public int hitTotal;
        public float perfectHitRate;
        public float goodHitRate;
        public float missHitRate;
        public int dodgeTotal;
        public float perfectDodgeRate;
        public float goodDodgeRate;
        public float missDodgeRate;
        public int timingSampleCount;
        public float averageTimingOffset;
        public float earlyInputRate;
        public float lateInputRate;
        public float enemyKillsPerMinute;
        public float scorePerMinute;
        public int maxCombo;
        public float averageComboLength;
        public int totalScore;
        public int enemyKillCount;
    }

    public sealed class TelemetryAnalysisSummary
    {
        public int fileCount;
        public int validRunCount;
        public int shortSessionCount;
        public readonly List<TelemetryRunMetrics> runs = new List<TelemetryRunMetrics>();
        public readonly List<TelemetryParseError> parseErrors = new List<TelemetryParseError>();
        public readonly List<string> warnings = new List<string>();
        public float averagePerfectHitRate;
        public float averageGoodHitRate;
        public float averageMissHitRate;
        public float averagePerfectDodgeRate;
        public float averageGoodDodgeRate;
        public float averageMissDodgeRate;
        public float averageTimingOffset;
        public float averageEarlyInputRate;
        public float averageLateInputRate;
        public float averageScorePerMinuteExcludingShort;
        public float averageEnemyKillsPerMinuteExcludingShort;
        public float averageMaxCombo;
        public float averageComboLength;
        public float averageRunDurationSeconds;
        public float averageNormalRunDurationSeconds;

        public string ToHumanReadableString()
        {
            return TelemetryAnalysis.ToHumanReadableSummary(this);
        }
    }
}
