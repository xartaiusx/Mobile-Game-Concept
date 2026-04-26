#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Game.Rhythm;

public class RhythmJudgementTests
{
    [Test]
    public void GradesMapToWindows()
    {
        var go = new GameObject("judge");
        var cfg = ScriptableObject.CreateInstance<RhythmConfig>();
        cfg.perfectWindow = 0.05f;
        cfg.goodWindow = 0.10f;
        var judge = go.AddComponent<RhythmJudgement>();
        judge.Configure(cfg);

        Assert.AreEqual(RhythmGrade.Perfect, judge.Judge(0.00));
        Assert.AreEqual(RhythmGrade.Perfect, judge.Judge(0.04));
        Assert.AreEqual(RhythmGrade.Good, judge.Judge(0.08));
        Assert.AreEqual(RhythmGrade.Miss, judge.Judge(0.20));
    }
}
#endif
