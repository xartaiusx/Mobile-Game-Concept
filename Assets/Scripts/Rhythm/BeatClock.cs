using UnityEngine;
using System;
using Game.Core;

namespace Game.Rhythm
{
    /// <summary>
    /// High precision beat clock driven by AudioSettings.dspTime.
    /// Emits OnBeat for integer beats and provides utilities for judging timing.
    /// </summary>
    public class BeatClock : MonoBehaviour
    {
        public static BeatClock Instance { get; private set; }

        [SerializeField] private RhythmConfig config;

        public event Action<int, double> OnBeat; // args: beatIndex, dspTimeAtBeat

        private double secondsPerBeat;
        private double firstBeatDspTime;
        private double nextBeatDspTime;
        private int beatIndex;
        private float levelSpeedMultiplier = 1f;
        private BaseCharacter levelSource;

        public double CurrentDspTime => AudioSettings.dspTime;
        public double SecondsPerBeat => secondsPerBeat;
        public int CurrentBeatIndex => beatIndex;
        public double CurrentPhase => BeatPhase(AudioSettings.dspTime);
        public float LevelSpeedMultiplier => levelSpeedMultiplier;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Recalculate();
        }

        private void Start()
        {
            ResetClock();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (config == null) return;
            RefreshLevelSpeedFromPlayer();
            double now = AudioSettings.dspTime;
            while (now >= nextBeatDspTime)
            {
                OnBeat?.Invoke(beatIndex, nextBeatDspTime);
                beatIndex++;
                nextBeatDspTime += secondsPerBeat;
            }
        }

        public void ResetClock()
        {
            if (config == null) return;
            beatIndex = 0;
            double start = AudioSettings.dspTime + config.dspOffsetSeconds;
            firstBeatDspTime = start;
            nextBeatDspTime = start;
        }

        public void Configure(RhythmConfig rhythmConfig)
        {
            config = rhythmConfig;
            levelSpeedMultiplier = 1f;
            Recalculate();
            ResetClock();
        }

        public void Recalculate()
        {
            float effectiveBpm = GetEffectiveBpm();
            if (effectiveBpm <= 0f)
            {
                secondsPerBeat = 0.5; // default to 120 BPM
                return;
            }
            secondsPerBeat = 60.0 / effectiveBpm;
        }

        public void SetLevelSpeedMultiplier(float multiplier)
        {
            float capped = config != null
                ? Mathf.Clamp(multiplier, 0.1f, Mathf.Max(0.1f, config.maxSpeedMultiplier))
                : Mathf.Max(0.1f, multiplier);

            if (Mathf.Approximately(levelSpeedMultiplier, capped))
                return;

            double now = AudioSettings.dspTime;
            double visualPhase = BeatPhase(now);
            levelSpeedMultiplier = capped;
            Recalculate();
            firstBeatDspTime = now - visualPhase * secondsPerBeat;
            nextBeatDspTime = now + (1d - visualPhase) * secondsPerBeat;
        }

        public void SetLevelSpeedFromLevel(int level)
        {
            if (config == null)
            {
                SetLevelSpeedMultiplier(1f);
                return;
            }

            int clampedLevel = Mathf.Max(1, level);
            SetLevelSpeedMultiplier(1f + (clampedLevel - 1) * Mathf.Max(0f, config.speedIncreasePerLevel));
        }

        public float GetEffectiveBpm()
        {
            float baseBpm = config != null && config.bpm > 0f ? config.bpm : 120f;
            return baseBpm * Mathf.Max(0.1f, levelSpeedMultiplier);
        }

        public double GetBeatPhase()
        {
            return BeatPhase(AudioSettings.dspTime);
        }

        public double GetCenteredBeatBarPhase()
        {
            double phase = GetBeatPhase() + 0.5d;
            return phase - System.Math.Floor(phase);
        }

        public float GetPerfectWindowNormalized()
        {
            if (config == null || secondsPerBeat <= 0d) return 0f;
            return Mathf.Clamp01((float)((config.perfectWindow * 2f) / secondsPerBeat));
        }

        public float GetGoodWindowNormalized()
        {
            if (config == null || secondsPerBeat <= 0d) return 0f;
            return Mathf.Clamp01((float)((config.goodWindow * 2f) / secondsPerBeat));
        }

        /// <summary>
        /// Returns signed time delta to the nearest beat in seconds.
        /// Negative means input happened before the nearest beat.
        /// </summary>
        public double TimeToNearestBeat(double dspTime)
        {
            if (secondsPerBeat <= 0) return double.MaxValue;
            double k = System.Math.Round((dspTime - firstBeatDspTime) / secondsPerBeat);
            double nearestBeatTime = firstBeatDspTime + k * secondsPerBeat;
            return dspTime - nearestBeatTime;
        }

        /// <summary>
        /// Returns absolute phase within the current beat [0,1).
        /// </summary>
        public double BeatPhase(double dspTime)
        {
            if (secondsPerBeat <= 0) return 0;
            double t = dspTime - firstBeatDspTime;
            double phase = t / secondsPerBeat;
            phase -= System.Math.Floor(phase);
            return phase;
        }

        private void RefreshLevelSpeedFromPlayer()
        {
            if (levelSource == null && PlayerManager.Instance != null && PlayerManager.Instance.Player != null)
                levelSource = PlayerManager.Instance.Player.GetComponent<BaseCharacter>();

            if (levelSource != null)
                SetLevelSpeedFromLevel(levelSource.Level);
        }
    }
}
