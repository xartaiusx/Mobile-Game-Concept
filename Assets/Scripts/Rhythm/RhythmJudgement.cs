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

        public RhythmConfig Config => config;

        public void Configure(RhythmConfig rhythmConfig)
        {
            config = rhythmConfig;
        }

        public RhythmGrade Judge(double signedDeltaSeconds)
        {
            if (config == null) return RhythmGrade.Miss;
            double abs = System.Math.Abs(signedDeltaSeconds);
            if (abs <= config.perfectWindow) return RhythmGrade.Perfect;
            if (abs <= config.goodWindow) return RhythmGrade.Good;
            return RhythmGrade.Miss;
        }

        public float DamageMultiplier(RhythmGrade g)
        {
            if (config == null) return 1f;
            switch (g)
            {
                case RhythmGrade.Perfect: return config.perfectDamageMultiplier;
                case RhythmGrade.Good: return config.goodDamageMultiplier;
                default: return config.missDamageMultiplier;
            }
        }

        public float CooldownRefund(RhythmGrade g)
        {
            if (config == null) return 0f;
            switch (g)
            {
                case RhythmGrade.Perfect: return config.perfectCooldownRefund;
                case RhythmGrade.Good: return config.goodCooldownRefund;
                default: return 0f;
            }
        }
    }
}
