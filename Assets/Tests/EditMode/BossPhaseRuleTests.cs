#if UNITY_EDITOR
using Game.AI.Enemies;
using Game.Combat;
using NUnit.Framework;
using UnityEngine;

public class BossPhaseRuleTests
{
    [Test]
    public void BossPhaseDataExposesReadablePhaseTuning()
    {
        var phase = ScriptableObject.CreateInstance<BossPhaseData>();
        phase.displayName = "Phase 2";
        phase.healthThreshold = 0.5f;
        phase.telegraphCooldownMultiplier = 0.75f;
        phase.telegraphWarningScale = 1.35f;

        Assert.AreEqual("Phase 2", phase.displayName);
        Assert.Less(phase.telegraphCooldownMultiplier, 1f);
        Assert.Greater(phase.telegraphWarningScale, 1f);
    }

    [Test]
    public void BossTelegraphPhaseTuningDoesNotThrow()
    {
        var controller = new GameObject("telegraph").AddComponent<BossTelegraphController>();

        Assert.DoesNotThrow(() => controller.ApplyPhaseTuning(0.75f, 1.25f));
    }
}
#endif
