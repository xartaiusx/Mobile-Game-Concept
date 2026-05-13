#if UNITY_EDITOR
using Game.Classes;
using Game.Core;
using NUnit.Framework;
using UnityEngine;

public class ArenaControllerTests
{
    [Test]
    public void ArenaStartsWithInitialWaveAndHidesBoss()
    {
        var playerGo = new GameObject("player");
        var player = playerGo.AddComponent<Fighter>();
        var enemyGo = new GameObject("enemy");
        var enemy = enemyGo.AddComponent<TestEnemy>();
        var bossGo = new GameObject("boss");
        bossGo.AddComponent<TestEnemy>();

        var arenaGo = new GameObject("arena");
        var arena = arenaGo.AddComponent<ArenaController>();
        arena.ConfigureForTests(player, new BaseEnemy[] { enemy }, bossGo);

        arena.BeginArena();

        Assert.AreEqual(ArenaState.Wave, arena.State);
        Assert.AreEqual(1, arena.ActiveEnemyCount);
        Assert.IsFalse(bossGo.activeSelf);
    }

    [Test]
    public void PlayerDeathSetsFailureState()
    {
        var playerGo = new GameObject("player");
        var player = playerGo.AddComponent<Fighter>();
        var arenaGo = new GameObject("arena");
        var arena = arenaGo.AddComponent<ArenaController>();
        arena.ConfigureForTests(player, null, null);

        arena.BeginArena();
        player.TakeDamage(999);

        Assert.AreEqual(ArenaState.Failure, arena.State);
    }

    [Test]
    public void AlphaWaveLimitCompletesSession()
    {
        var playerGo = new GameObject("player");
        var player = playerGo.AddComponent<Fighter>();
        var enemyGo = new GameObject("enemy");
        var enemy = enemyGo.AddComponent<TestEnemy>();
        var scaler = new GameObject("scaler").AddComponent<DifficultyScaler>();

        var arenaGo = new GameObject("arena");
        var arena = arenaGo.AddComponent<ArenaController>();
        arena.ConfigureForTests(player, new BaseEnemy[] { enemy }, null, scaler);
        arena.ConfigureSessionWaveLimitForTests(1);

        arena.BeginArena();
        enemy.TakeDamage(999);

        Assert.AreEqual(ArenaState.Victory, arena.State);
        Assert.That(arena.LastStateMessage, Does.Contain("Session Complete"));
    }

    [Test]
    public void EndlessWaveClearAdvancesToNextWaveWhenSessionLimitDisabled()
    {
        var playerGo = new GameObject("player");
        var player = playerGo.AddComponent<Fighter>();
        var enemyGo = new GameObject("enemy");
        var enemy = enemyGo.AddComponent<TestEnemy>();
        var scaler = new GameObject("scaler").AddComponent<DifficultyScaler>();

        var arenaGo = new GameObject("arena");
        var arena = arenaGo.AddComponent<ArenaController>();
        arena.ConfigureForTests(player, new BaseEnemy[] { enemy }, null, scaler);
        arena.ConfigureSessionWaveLimitForTests(0);

        arena.BeginArena();
        enemy.TakeDamage(999);

        Assert.AreEqual(ArenaState.WaveCleared, arena.State);
        Assert.AreNotEqual(ArenaState.Victory, arena.State);

        arena.BeginNextWaveForTests();

        Assert.AreEqual(2, arena.CurrentWave);
        Assert.AreEqual(ArenaState.Wave, arena.State);
    }

    [Test]
    public void RestartedEndlessRunResetsToWaveOne()
    {
        var playerGo = new GameObject("player");
        var player = playerGo.AddComponent<Fighter>();
        var enemyGo = new GameObject("enemy");
        var enemy = enemyGo.AddComponent<TestEnemy>();
        var scaler = new GameObject("scaler").AddComponent<DifficultyScaler>();

        var arenaGo = new GameObject("arena");
        var arena = arenaGo.AddComponent<ArenaController>();
        arena.ConfigureForTests(player, new BaseEnemy[] { enemy }, null, scaler);
        arena.ConfigureSessionWaveLimitForTests(0);

        arena.BeginArena();
        enemy.TakeDamage(999);
        arena.BeginNextWaveForTests();
        Assert.AreEqual(2, arena.CurrentWave);

        arena.ConfigureForTests(player, null, null, scaler);
        arena.ConfigureSessionWaveLimitForTests(0);
        arena.BeginArena();

        Assert.AreEqual(1, arena.CurrentWave);
        Assert.AreNotEqual(ArenaState.Victory, arena.State);
    }

    [Test]
    public void DifficultyScalerIncreasesValuesAndCaps()
    {
        var scaler = new GameObject("scaler").AddComponent<DifficultyScaler>();

        DifficultySnapshot wave1 = scaler.Evaluate(1, false);
        DifficultySnapshot wave20 = scaler.Evaluate(20, false);
        DifficultySnapshot wave200 = scaler.Evaluate(200, false);

        Assert.Greater(wave20.enemyCount, wave1.enemyCount);
        Assert.Greater(wave20.healthMultiplier, wave1.healthMultiplier);
        Assert.Greater(wave20.damageMultiplier, wave1.damageMultiplier);
        Assert.Greater(wave20.beatSpeedMultiplier, wave1.beatSpeedMultiplier);
        Assert.Less(wave20.attackCooldownMultiplier, wave1.attackCooldownMultiplier);
        Assert.LessOrEqual(wave200.enemyCount, 8);
        Assert.LessOrEqual(wave200.healthMultiplier, 2.75f);
        Assert.LessOrEqual(wave200.beatSpeedMultiplier, 1.35f);
        Assert.GreaterOrEqual(wave200.attackCooldownMultiplier, 0.72f);
    }

    private class TestEnemy : BaseEnemy
    {
        public override void PerformAttack()
        {
        }
    }
}
#endif
