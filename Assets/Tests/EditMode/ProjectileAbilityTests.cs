#if UNITY_EDITOR
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using NUnit.Framework;
using UnityEngine;

public class ProjectileAbilityTests
{
    [Test]
    public void ProjectileLikeAbilityUsesRhythmDamageScaling()
    {
        var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
        ability.executionStyle = AbilityExecutionStyle.ProjectileLike;
        ability.baseDamage = 20;
        ability.rhythmScaling = new AbilityRhythmScaling
        {
            perfectMultiplier = 2f,
            goodMultiplier = 1f,
            missMultiplier = 0.5f
        };

        Assert.AreEqual(40, ability.ScaledDamage(RhythmGrade.Perfect));
        Assert.AreEqual(20, ability.ScaledDamage(RhythmGrade.Good));
        Assert.AreEqual(10, ability.ScaledDamage(RhythmGrade.Miss));
    }

    [Test]
    public void AbilityProjectileInitializationDoesNotThrow()
    {
        var projectileGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var projectile = projectileGo.AddComponent<PoolableProjectile>();

        Assert.DoesNotThrow(() => projectile.InitializeAbilityProjectile(Vector3.forward * 5f, 10f, 12, null, null, RhythmGrade.Good, false, 0f, 0f, null));
    }
}
#endif
