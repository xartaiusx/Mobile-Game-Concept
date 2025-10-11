using UnityEngine;

namespace Game.Rhythm
{
    public enum RhythmGrade { Miss, Good, Perfect }

    /// <summary>
    /// Converts time error to a grade and exposes combat multipliers.
    /// </summary>
    [DefaultExecutionOrder(5)]
    public class RhythmJudgement : MonoBehaviour
    {
        [SerializeField] private RhythmConfig config;

        public RhythmGrade Judge(double signedDeltaSeconds)
        {
            double abs = System.Math.Abs(signedDeltaSeconds);
            if (abs <= config.perfectWindow) return RhythmGrade.Perfect;
            if (abs <= config.goodWindow) return RhythmGrade.Good;
            return RhythmGrade.Miss;
        }

        public float DamageMultiplier(RhythmGrade g)
        {
            switch (g)
            {
                case RhythmGrade.Perfect: return config.perfectDamageMultiplier;
                case RhythmGrade.Good: return config.goodDamageMultiplier;
                default: return config.missDamageMultiplier;
            }
        }

        public float CooldownRefund(RhythmGrade g)
        {
            switch (g)
            {
                case RhythmGrade.Perfect: return config.perfectCooldownRefund;
                case RhythmGrade.Good: return config.goodCooldownRefund;
                default: return 0f;
            }
        }
    }
}
