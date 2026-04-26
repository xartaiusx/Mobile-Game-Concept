#if UNITY_EDITOR
using System.IO;
using Game.Classes;
using Game.Core;
using Game.Systems;
using NUnit.Framework;
using UnityEngine;

public class TelemetryManagerTests
{
    private string tempRoot;

    [SetUp]
    public void SetUp()
    {
        tempRoot = Path.Combine(Application.temporaryCachePath, "TelemetryManagerTests");
        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, true);
        Directory.CreateDirectory(tempRoot);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            Object.DestroyImmediate(go);

        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, true);
    }

    [Test]
    public void NoBatchLabelPreservesRootTelemetryPath()
    {
        TelemetryManager telemetry = CreateTelemetryManager();
        telemetry.ConfigureTelemetryRootForTests(tempRoot);
        telemetry.SetBatchLabel(string.Empty);
        telemetry.ResetRun();

        string path = telemetry.WriteRunSummary();

        Assert.IsNotEmpty(path);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(Path.Combine(tempRoot, "Telemetry"), Path.GetDirectoryName(path));
        Assert.That(Path.GetFileName(path), Does.StartWith("run_"));
        Assert.That(Path.GetFileName(path), Does.Not.Contain("warrior_batch"));
    }

    [Test]
    public void BatchLabelWritesToBatchFolderAndSnapshot()
    {
        TelemetryManager telemetry = CreateTelemetryManager();
        telemetry.ConfigureTelemetryRootForTests(tempRoot);
        telemetry.SetBatchLabel("warrior batch 001");
        telemetry.SetRunNotes("baseline pass");
        telemetry.ResetRun();

        string path = telemetry.WriteRunSummary();
        TelemetrySnapshot snapshot = JsonUtility.FromJson<TelemetrySnapshot>(File.ReadAllText(path));

        Assert.IsTrue(path.Replace('\\', '/').Contains("/Telemetry/warrior_batch_001/"));
        Assert.That(Path.GetFileName(path), Does.Contain("warrior_batch_001"));
        Assert.AreEqual("warrior_batch_001", snapshot.batchLabel);
        Assert.AreEqual("baseline pass", snapshot.runNotes);
    }

    [Test]
    public void WarriorPlayerClassSceneAndDurationArePopulated()
    {
        GameObject playerManagerObject = new GameObject("PlayerManager");
        var playerManager = playerManagerObject.AddComponent<PlayerManager>();
        GameObject player = new GameObject("Player_Warrior");
        var fighter = player.AddComponent<Fighter>();
        fighter.InitializeStats();
        playerManager.RegisterPlayer(player.transform);

        TelemetryManager telemetry = CreateTelemetryManager();
        telemetry.ConfigureTelemetryRootForTests(tempRoot);
        telemetry.ResetRun();
        string path = telemetry.WriteRunSummary();
        TelemetrySnapshot snapshot = JsonUtility.FromJson<TelemetrySnapshot>(File.ReadAllText(path));

        Assert.IsNotNull(fighter);
        Assert.AreEqual("Warrior", snapshot.playerClass);
        Assert.IsNotEmpty(snapshot.sceneName);
        Assert.GreaterOrEqual(snapshot.runDurationSeconds, 0f);
    }

    [Test]
    public void WriteFailureIsHandledSafely()
    {
        string fileAsRoot = Path.Combine(tempRoot, "not-a-directory");
        File.WriteAllText(fileAsRoot, "blocking file");
        TelemetryManager telemetry = CreateTelemetryManager();
        telemetry.ConfigureTelemetryRootForTests(fileAsRoot);
        telemetry.SuppressWriteWarningsForTests(true);
        telemetry.ResetRun();

        string path = telemetry.WriteRunSummary();

        Assert.IsEmpty(path);
        Assert.IsEmpty(telemetry.LastWrittenPath);
    }

    private static TelemetryManager CreateTelemetryManager()
    {
        GameObject go = new GameObject("TelemetryManager");
        return go.AddComponent<TelemetryManager>();
    }
}
#endif
