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
        private double nextBeatDspTime;
        private int beatIndex;

        public double CurrentDspTime => AudioSettings.dspTime;
        public double SecondsPerBeat => secondsPerBeat;

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
            // Compute k such that beat time is closest to dspTime
            double k = System.Math.Round((dspTime - (nextBeatDspTime - secondsPerBeat * beatIndex)) / secondsPerBeat + beatIndex);
            double nearestBeatTime = (nextBeatDspTime - secondsPerBeat * beatIndex) + k * secondsPerBeat;
            return dspTime - nearestBeatTime;
        }

        /// <summary>
        /// Returns absolute phase within the current beat [0,1).
        /// </summary>
        public double BeatPhase(double dspTime)
        {
            double beatStartTime = (nextBeatDspTime - secondsPerBeat * beatIndex);
            double t = dspTime - beatStartTime;
            double phase = t / secondsPerBeat;
            phase -= System.Math.Floor(phase);
            return phase;
        }
    }
}
