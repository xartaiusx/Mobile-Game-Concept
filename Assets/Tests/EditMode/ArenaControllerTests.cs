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

    private class TestEnemy : BaseEnemy
    {
        public override void PerformAttack()
        {
        }
    }
}
#endif
