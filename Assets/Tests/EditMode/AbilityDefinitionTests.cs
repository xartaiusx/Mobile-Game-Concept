#if UNITY_EDITOR
using Game.Combat;
using Game.Rhythm;
using NUnit.Framework;
using UnityEngine;

public class AbilityDefinitionTests
{
    [Test]
    public void RhythmScalingAppliesToDamageAndHealing()
    {
        var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
        ability.baseDamage = 20;
        ability.healAmount = 10;
        ability.rhythmScaling = new AbilityRhythmScaling
        {
            perfectMultiplier = 2f,
            goodMultiplier = 1.25f,
            missMultiplier = 0.5f
        };

        Assert.AreEqual(40, ability.ScaledDamage(RhythmGrade.Perfect));
        Assert.AreEqual(25, ability.ScaledDamage(RhythmGrade.Good));
        Assert.AreEqual(10, ability.ScaledDamage(RhythmGrade.Miss));
        Assert.AreEqual(20, ability.ScaledHealing(RhythmGrade.Perfect));
    }
}
#endif
