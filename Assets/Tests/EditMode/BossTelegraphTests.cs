#if UNITY_EDITOR
using Game.Combat;
using NUnit.Framework;
using UnityEngine;

public class BossTelegraphTests
{
    [Test]
    public void TelegraphCountsDownByBeats()
    {
        var go = new GameObject("boss");
        var controller = go.AddComponent<BossTelegraphController>();
        var data = ScriptableObject.CreateInstance<BossTelegraphData>();
        data.beatsBeforeImpact = 2;

        Assert.IsTrue(controller.BeginTelegraph(data));
        Assert.AreEqual(2, controller.RemainingBeats);

        controller.TickBeatForTests();
        Assert.IsTrue(controller.IsTelegraphing);
        Assert.AreEqual(1, controller.RemainingBeats);

        controller.TickBeatForTests();
        Assert.IsFalse(controller.IsTelegraphing);
    }
}
#endif
