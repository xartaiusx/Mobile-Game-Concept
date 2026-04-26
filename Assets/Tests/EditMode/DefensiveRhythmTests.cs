#if UNITY_EDITOR
using Game.Classes;
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using NUnit.Framework;
using UnityEngine;

public class DefensiveRhythmTests
{
    [Test]
    public void PerfectParryCancelsParryableDamage()
    {
        var go = new GameObject("fighter");
        var fighter = go.AddComponent<Fighter>();
        var parry = go.AddComponent<ParryController>();
        int startingHealth = fighter.CurrentHealth;

        parry.ResolveParryForTests(RhythmGrade.Perfect);
        fighter.TakeDamage(new DamageContext(null, 20, DamageType.Physical, RhythmGrade.Miss, true));

        Assert.AreEqual(startingHealth, fighter.CurrentHealth);
    }

    [Test]
    public void GoodParryReducesDamage()
    {
        var go = new GameObject("fighter");
        var fighter = go.AddComponent<Fighter>();
        var parry = go.AddComponent<ParryController>();
        int startingHealth = fighter.CurrentHealth;

        parry.ResolveParryForTests(RhythmGrade.Good);
        fighter.TakeDamage(new DamageContext(null, 20, DamageType.Physical, RhythmGrade.Miss, true));

        Assert.Greater(fighter.CurrentHealth, startingHealth - 20);
        Assert.Less(fighter.CurrentHealth, startingHealth);
    }
}
#endif
