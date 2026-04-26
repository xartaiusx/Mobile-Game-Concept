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

    [Test]
    public void TelegraphEmitsCountdownAndImpactEvents()
    {
        var go = new GameObject("boss-events");
        var controller = go.AddComponent<BossTelegraphController>();
        var data = ScriptableObject.CreateInstance<BossTelegraphData>();
        data.displayName = "Test Slam";
        data.beatsBeforeImpact = 1;

        int starts = 0;
        int impacts = 0;
        controller.TelegraphStarted += (_, beats, __) =>
        {
            starts++;
            Assert.AreEqual(1, beats);
        };
        controller.TelegraphImpacted += (_, __) => impacts++;

        Assert.IsTrue(controller.BeginTelegraph(data));
        controller.TickBeatForTests();

        Assert.AreEqual(1, starts);
        Assert.AreEqual(1, impacts);
    }
}
#endif
