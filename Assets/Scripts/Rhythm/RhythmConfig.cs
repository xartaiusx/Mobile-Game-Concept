using UnityEngine;

namespace Game.Rhythm
{
    [CreateAssetMenu(menuName = "Game/Rhythm/RhythmConfig")]
    public class RhythmConfig : ScriptableObject
    {
        [Header("Global Music Timing")]
        [Tooltip("Beats per minute of the current track.")]
        public float bpm = 120f;

        [Tooltip("Offset in seconds from track start to first beat. Use this to align visuals with audio.")]
        public double dspOffsetSeconds = 0.0;

        [Header("Judgement Windows (seconds)")]
        [Tooltip("Perfect window half width. Input within ± this time is Perfect.")]
        public float perfectWindow = 0.050f;

        [Tooltip("Good window half width. Input within ± this time is Good when not Perfect.")]
        public float goodWindow = 0.100f;

        [Tooltip("Extra seconds granted to early inputs for mobile latency and human anticipation.")]
        public float earlyInputBiasSeconds = 0.015f;

        [Tooltip("Extra seconds granted to late inputs. Usually smaller than early bias.")]
        public float lateInputBiasSeconds = 0.005f;

        [Header("Level Speed Scaling")]
        [Tooltip("Additional slider/BPM speed per player level above 1.")]
        public float speedIncreasePerLevel = 0.025f;

        [Tooltip("Maximum effective slider/BPM multiplier.")]
        public float maxSpeedMultiplier = 1.5f;

        [Header("Multipliers and Bonuses")]
        public float perfectDamageMultiplier = 1.5f;
        public float goodDamageMultiplier = 1.15f;
        public float missDamageMultiplier = 0.75f;

        [Header("Score")]
        public int perfectScore = 100;
        public int goodScore = 50;
        public int missScore = 10;
        public int comboScoreBonus = 5;

        [Tooltip("Cooldown refund percent on Perfect. 0.2 means 20 percent.")]
        [Range(0f, 1f)] public float perfectCooldownRefund = 0.2f;

        [Tooltip("Cooldown refund percent on Good.")]
        [Range(0f, 1f)] public float goodCooldownRefund = 0.1f;
    }
}
