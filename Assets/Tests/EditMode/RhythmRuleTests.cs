#if UNITY_EDITOR
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using Game.UI;
using NUnit.Framework;
using UnityEngine;

public class RhythmRuleTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            Object.DestroyImmediate(go);
    }

    [Test]
    public void RhythmJudgementOnlyReturnsPerfectInsidePerfectWindow()
    {
        var config = ScriptableObject.CreateInstance<RhythmConfig>();
        config.perfectWindow = 0.05f;
        config.goodWindow = 0.10f;
        var judgement = new GameObject("judgement").AddComponent<RhythmJudgement>();
        judgement.Configure(config);

        Assert.AreEqual(RhythmGrade.Perfect, judgement.Judge(0.049));
        Assert.AreEqual(RhythmGrade.Good, judgement.Judge(0.075));
        Assert.AreEqual(RhythmGrade.Miss, judgement.Judge(0.125));
    }

    [Test]
    public void EffectiveBpmIncreasesWithLevelAndCaps()
    {
        var config = ScriptableObject.CreateInstance<RhythmConfig>();
        config.bpm = 120f;
        config.speedIncreasePerLevel = 0.1f;
        config.maxSpeedMultiplier = 1.3f;

        var clock = new GameObject("clock").AddComponent<BeatClock>();
        clock.Configure(config);

        clock.SetLevelSpeedFromLevel(1);
        Assert.AreEqual(120f, clock.GetEffectiveBpm(), 0.01f);

        clock.SetLevelSpeedFromLevel(3);
        Assert.AreEqual(144f, clock.GetEffectiveBpm(), 0.01f);

        clock.SetLevelSpeedFromLevel(9);
        Assert.AreEqual(156f, clock.GetEffectiveBpm(), 0.01f);
    }

    [Test]
    public void PerfectWindowNormalizedMatchesCenteredBeatBarMath()
    {
        var config = ScriptableObject.CreateInstance<RhythmConfig>();
        config.bpm = 120f;
        config.perfectWindow = 0.05f;

        var clock = new GameObject("clock").AddComponent<BeatClock>();
        clock.Configure(config);

        Assert.AreEqual(0.2f, clock.GetPerfectWindowNormalized(), 0.001f);
    }

    [Test]
    public void PerfectAttackGivesHigherDamageAndScoreThanGoodAndMiss()
    {
        var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
        ability.baseDamage = 20;
        ability.rhythmScaling = new AbilityRhythmScaling { perfectMultiplier = 1.5f, goodMultiplier = 1.15f, missMultiplier = 0.75f };

        var score = new GameObject("score").AddComponent<ScoreSystem>();
        var config = ScriptableObject.CreateInstance<RhythmConfig>();
        score.Configure(config);

        Assert.Greater(ability.ScaledDamage(RhythmGrade.Perfect), ability.ScaledDamage(RhythmGrade.Good));
        Assert.Greater(ability.ScaledDamage(RhythmGrade.Good), ability.ScaledDamage(RhythmGrade.Miss));
        Assert.Greater(score.CalculateHitScore(RhythmGrade.Perfect, 1), score.CalculateHitScore(RhythmGrade.Good, 1));
        Assert.Greater(score.CalculateHitScore(RhythmGrade.Good, 1), score.CalculateHitScore(RhythmGrade.Miss, 1));
    }

    [Test]
    public void PerfectDodgeCooldownIsLowerThanMissCooldown()
    {
        var perfectGo = new GameObject("perfect-dodge");
        var perfect = perfectGo.AddComponent<DodgeController>();
        perfect.ResolveDodgeForTests(RhythmGrade.Perfect, Vector3.forward);

        var missGo = new GameObject("miss-dodge");
        var miss = missGo.AddComponent<DodgeController>();
        miss.ResolveDodgeForTests(RhythmGrade.Miss, Vector3.forward);

        Assert.Less(perfect.LastResolvedCooldown, miss.LastResolvedCooldown);
        Assert.Greater(perfect.LastResolvedSpeedMultiplier, miss.LastResolvedSpeedMultiplier);
    }

    [Test]
    public void BeatBarCanInitializeWithoutSceneReferences()
    {
        var root = new GameObject("beat-bar");
        root.AddComponent<Canvas>();
        var bar = new GameObject("bar").AddComponent<RectTransform>();
        bar.SetParent(root.transform);
        var marker = new GameObject("marker").AddComponent<RectTransform>();
        marker.SetParent(bar);
        var perfect = new GameObject("PerfectWindow").AddComponent<RectTransform>();
        perfect.SetParent(bar);
        var beatBar = root.AddComponent<BeatBarUI>();

        Assert.DoesNotThrow(() => beatBar.ShowFeedback(RhythmGrade.Perfect));
    }
}
#endif
