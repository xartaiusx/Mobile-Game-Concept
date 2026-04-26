#if UNITY_EDITOR
using Game.Combat;
using NUnit.Framework;
using UnityEngine;

public class AttackTimingDataTests
{
    [Test]
    public void TotalDurationIncludesWindupActiveAndRecovery()
    {
        var timing = ScriptableObject.CreateInstance<AttackTimingData>();
        timing.windupSeconds = 0.1f;
        timing.activeSeconds = 0.2f;
        timing.recoverySeconds = 0.3f;

        Assert.AreEqual(0.6f, timing.TotalDuration, 0.0001f);
    }

    [Test]
    public void DefaultsAreAnimatorReady()
    {
        var timing = ScriptableObject.CreateInstance<AttackTimingData>();

        Assert.GreaterOrEqual(timing.windupSeconds, 0f);
        Assert.Greater(timing.activeSeconds, 0f);
        Assert.GreaterOrEqual(timing.recoverySeconds, 0f);
        Assert.IsTrue(timing.beatAlignedImpact);
    }
}
#endif
