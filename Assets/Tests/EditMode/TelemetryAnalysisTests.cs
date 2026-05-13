#if UNITY_EDITOR
using System.IO;
using Game.Editor;
using Game.Systems;
using NUnit.Framework;
using UnityEngine;

public class TelemetryAnalysisTests
{
    [Test]
    public void ValidTelemetryJsonParsesCorrectly()
    {
        TelemetryParseResult result = TelemetryAnalysis.ParseJson(HealthyJson(60f));

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(8, result.snapshot.perfectHitCount);
        Assert.AreEqual(60f, result.snapshot.runDurationSeconds);
        Assert.AreEqual("Warrior", result.snapshot.playerClass);
    }

    [Test]
    public void MissingOptionalFieldsDoNotCrash()
    {
        string json = "{\"perfectHitCount\":1,\"goodHitCount\":1,\"missHitCount\":0,\"totalSurvivalTime\":45}";

        TelemetryRunMetrics metrics = TelemetryAnalysis.CreateRunMetrics(TelemetryAnalysis.ParseJson(json).snapshot);

        Assert.AreEqual(2, metrics.hitTotal);
        Assert.AreEqual(0.5f, metrics.perfectHitRate, 0.001f);
        Assert.AreEqual(45f, metrics.runDurationSeconds, 0.001f);
    }

    [Test]
    public void MalformedJsonIsReportedSafely()
    {
        TelemetryParseResult result = TelemetryAnalysis.ParseJson("{not valid json");

        Assert.IsFalse(result.IsValid);
        Assert.IsNotEmpty(result.errorMessage);
    }

    [Test]
    public void DerivedMetricsCalculateRatesAndTimingBias()
    {
        TelemetryRunMetrics metrics = TelemetryAnalysis.CreateRunMetrics(TelemetryAnalysis.ParseJson(HealthyJson(60f)).snapshot);

        Assert.AreEqual(28, metrics.hitTotal);
        Assert.AreEqual(8f / 28f, metrics.perfectHitRate, 0.001f);
        Assert.AreEqual(14f / 28f, metrics.goodHitRate, 0.001f);
        Assert.AreEqual(6f / 28f, metrics.missHitRate, 0.001f);
        Assert.AreEqual(10, metrics.dodgeTotal);
        Assert.AreEqual(0.2f, metrics.perfectDodgeRate, 0.001f);
        Assert.AreEqual(0.3f, metrics.missDodgeRate, 0.001f);
        Assert.AreEqual(0.5f, metrics.earlyInputRate, 0.001f);
        Assert.AreEqual(0.5f, metrics.lateInputRate, 0.001f);
    }

    [Test]
    public void ShortSessionsAreDetectedAndExcludedFromNormalScoreRate()
    {
        TelemetryAnalysisSummary summary = TelemetryAnalysis.AnalyzeSnapshots(new[]
        {
            Snapshot(10f, 10000f, 2, 2, 1, 1, 2, 1, 0f, 4f, 5),
            Snapshot(60f, 600f, 8, 14, 6, 2, 5, 3, 0f, 4f, 5)
        });

        Assert.AreEqual(1, summary.shortSessionCount);
        Assert.AreEqual(600f, summary.averageScorePerMinuteExcludingShort, 0.001f);
    }

    [Test]
    public void DirectoryAnalysisIgnoresMalformedFilesAndReportsErrors()
    {
        string directory = Path.Combine(Application.temporaryCachePath, "TelemetryAnalysisTests");
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "valid.json"), HealthyJson(60f));
            File.WriteAllText(Path.Combine(directory, "broken.json"), "{nope");

            TelemetryAnalysisSummary summary = TelemetryAnalysis.AnalyzeDirectory(directory);

            Assert.AreEqual(2, summary.fileCount);
            Assert.AreEqual(1, summary.validRunCount);
            Assert.AreEqual(1, summary.parseErrors.Count);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void DirectoryAnalysisCanIncludeBatchFoldersAndParseBatchLabels()
    {
        string directory = Path.Combine(Application.temporaryCachePath, "TelemetryBatchAnalysisTests");
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "run_root.json"), HealthyJson(60f));
            string batchDirectory = Path.Combine(directory, "warrior_batch_001");
            Directory.CreateDirectory(batchDirectory);
            File.WriteAllText(Path.Combine(batchDirectory, "run_batch.json"), HealthyJson(75f, "warrior_batch_001"));

            TelemetryAnalysisSummary rootOnly = TelemetryAnalysis.AnalyzeDirectory(directory, false);
            TelemetryAnalysisSummary allBatches = TelemetryAnalysis.AnalyzeDirectory(directory, true);
            TelemetryAnalysisSummary batchOnly = TelemetryAnalysis.AnalyzeDirectory(batchDirectory, false);

            Assert.AreEqual(1, rootOnly.validRunCount);
            Assert.AreEqual(2, allBatches.validRunCount);
            Assert.AreEqual(1, batchOnly.validRunCount);
            Assert.Contains("warrior_batch_001", allBatches.batchLabels);
            Assert.IsTrue(ContainsBatchLabel(allBatches, "warrior_batch_001"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void AlphaRunJsonParsesEndReasonAndBatchLabel()
    {
        TelemetrySnapshot snapshot = Snapshot(45f, 500f, 6, 10, 4, 2, 4, 2, 0.01f, 3.5f, 4);
        snapshot.batchLabel = "alpha_warrior_batch_001";
        snapshot.runEnded = true;
        snapshot.runEndReason = "session_complete";

        TelemetryParseResult result = TelemetryAnalysis.ParseJson(JsonUtility.ToJson(snapshot), "alpha_run.json");
        TelemetryRunMetrics metrics = TelemetryAnalysis.CreateRunMetrics(result.snapshot, "alpha_run.json");

        Assert.IsTrue(result.IsValid, result.errorMessage);
        Assert.IsTrue(result.snapshot.runEnded);
        Assert.AreEqual("session_complete", result.snapshot.runEndReason);
        Assert.AreEqual("alpha_warrior_batch_001", metrics.batchLabel);
        Assert.AreEqual(45f, metrics.runDurationSeconds, 0.001f);
    }

    [Test]
    public void HighMissRateProducesWarning()
    {
        TelemetryAnalysisSummary summary = TelemetryAnalysis.AnalyzeSnapshots(new[]
        {
            Snapshot(60f, 600f, 2, 4, 10, 2, 5, 3, 0f, 4f, 5)
        });

        string warnings = string.Join("\n", summary.warnings);
        Assert.That(warnings, Does.Contain("Miss rate is high"));
        Assert.That(warnings, Does.Contain("Observed:"));
        Assert.That(warnings, Does.Contain("Target:"));
        Assert.That(warnings, Does.Contain("Inspect: RhythmConfig.goodWindow"));
    }

    [Test]
    public void LowPerfectDodgeRateProducesWarning()
    {
        TelemetryAnalysisSummary summary = TelemetryAnalysis.AnalyzeSnapshots(new[]
        {
            Snapshot(60f, 600f, 8, 14, 6, 0, 7, 3, 0f, 4f, 5)
        });

        string warnings = string.Join("\n", summary.warnings);
        Assert.That(warnings, Does.Contain("Perfect dodge rate is too low"));
        Assert.That(warnings, Does.Contain("Inspect: dodge timing window"));
    }

    [Test]
    public void TimingBiasWarningsTrigger()
    {
        TelemetryAnalysisSummary early = TelemetryAnalysis.AnalyzeSnapshots(new[]
        {
            Snapshot(60f, 600f, 8, 14, 6, 2, 5, 3, -0.07f, 4f, 5)
        });
        TelemetryAnalysisSummary late = TelemetryAnalysis.AnalyzeSnapshots(new[]
        {
            Snapshot(60f, 600f, 8, 14, 6, 2, 5, 3, 0.07f, 4f, 5)
        });

        Assert.That(string.Join("\n", early.warnings), Does.Contain("Average input is early"));
        Assert.That(string.Join("\n", early.warnings), Does.Contain("Inspect: RhythmConfig.earlyInputBiasSeconds"));
        Assert.That(string.Join("\n", late.warnings), Does.Contain("Average input is late"));
        Assert.That(string.Join("\n", late.warnings), Does.Contain("Inspect: RhythmConfig.lateInputBiasSeconds"));
    }

    [Test]
    public void HealthySampleDataProducesNoSevereTuningWarnings()
    {
        TelemetryAnalysisSummary summary = TelemetryAnalysis.AnalyzeSnapshots(new[]
        {
            Snapshot(60f, 600f, 8, 14, 6, 2, 5, 3, 0.01f, 4f, 5),
            Snapshot(75f, 720f, 10, 18, 7, 3, 7, 4, -0.01f, 4.5f, 6)
        });

        Assert.IsEmpty(summary.warnings);
    }

    [Test]
    public void RuntimeAnalysisAssemblyDoesNotDependOnUnityEditor()
    {
        Assert.IsNull(typeof(TelemetryAnalysis).Assembly.GetReferencedAssemblies()
            .FirstOrDefaultName("UnityEditor"));
    }

    [Test]
    public void TelemetryAnalyzerEditorValidationHandlesEmptyAndSampleFolders()
    {
        Assert.IsTrue(TelemetryAnalysisWindow.ValidateAnalyzer());
    }

    private static TelemetrySnapshot Snapshot(
        float duration,
        float scorePerMinute,
        int perfectHits,
        int goodHits,
        int missHits,
        int perfectDodges,
        int goodDodges,
        int missDodges,
        float averageTimingOffset,
        float averageComboLength,
        int maxCombo)
    {
        return new TelemetrySnapshot
        {
            schemaVersion = 3,
            runStartedAtUtc = "2026-04-26T12:00:00.0000000Z",
            runEndedAtUtc = "2026-04-26T12:01:00.0000000Z",
            runDurationSeconds = duration,
            sceneName = "VerticalSlice",
            playerClass = "Warrior",
            batchLabel = string.Empty,
            runNotes = string.Empty,
            perfectHitCount = perfectHits,
            goodHitCount = goodHits,
            missHitCount = missHits,
            perfectDodgeCount = perfectDodges,
            goodDodgeCount = goodDodges,
            missDodgeCount = missDodges,
            timingOffsets = new[] { -0.02f, -0.01f, 0.01f, 0.02f },
            averageTimingOffset = averageTimingOffset,
            totalSurvivalTime = duration,
            enemyKillCount = 6,
            maxCombo = maxCombo,
            averageComboLength = averageComboLength,
            totalScore = Mathf.RoundToInt(scorePerMinute * duration / 60f),
            scorePerMinute = scorePerMinute
        };
    }

    private static string HealthyJson(float duration, string batchLabel = "")
    {
        TelemetrySnapshot snapshot = Snapshot(duration, 600f, 8, 14, 6, 2, 5, 3, 0.01f, 4f, 5);
        snapshot.batchLabel = batchLabel;
        return JsonUtility.ToJson(snapshot);
    }

    private static bool ContainsBatchLabel(TelemetryAnalysisSummary summary, string batchLabel)
    {
        for (int i = 0; i < summary.runs.Count; i++)
        {
            if (summary.runs[i].batchLabel == batchLabel)
                return true;
        }

        return false;
    }
}

public static class TelemetryTestAssemblyExtensions
{
    public static System.Reflection.AssemblyName FirstOrDefaultName(this System.Reflection.AssemblyName[] names, string name)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].Name == name)
                return names[i];
        }

        return null;
    }
}
#endif
