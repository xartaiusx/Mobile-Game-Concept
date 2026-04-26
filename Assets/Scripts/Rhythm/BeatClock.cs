using UnityEngine;
using System;

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

        public double CurrentDspTime => AudioSettings.dspTime;
        public double SecondsPerBeat => secondsPerBeat;
        public int CurrentBeatIndex => beatIndex;
        public double CurrentPhase => BeatPhase(AudioSettings.dspTime);

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

        public void Recalculate()
        {
            if (config == null || config.bpm <= 0f)
            {
                secondsPerBeat = 0.5; // default to 120 BPM
                return;
            }
            secondsPerBeat = 60.0 / config.bpm;
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
    }
}
