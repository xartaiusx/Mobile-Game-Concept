#if UNITY_EDITOR
using Game.Classes;
using Game.Core;
using NUnit.Framework;
using UnityEngine;

public class DamageContextTests
{
    [Test]
    public void TakeDamageIntStillReducesHealth()
    {
        var go = new GameObject("fighter");
        var fighter = go.AddComponent<Fighter>();
        int startingHealth = fighter.CurrentHealth;

        fighter.TakeDamage(7);

        Assert.AreEqual(startingHealth - 7, fighter.CurrentHealth);
    }

    [Test]
    public void TakeDamageContextReducesHealth()
    {
        var go = new GameObject("fighter");
        var fighter = go.AddComponent<Fighter>();
        int startingHealth = fighter.CurrentHealth;

        fighter.TakeDamage(new DamageContext(null, 9, DamageType.Magical));

        Assert.AreEqual(startingHealth - 9, fighter.CurrentHealth);
    }
}
#endif
