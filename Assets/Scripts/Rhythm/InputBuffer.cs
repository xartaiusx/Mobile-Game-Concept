using UnityEngine;
using System;

namespace Game.Rhythm
{
    /// <summary>
    /// Buffers a button press within a small window and resolves it on the next beat tick.
    /// Subscribe to OnResolved to perform the action with a judgement grade.
    /// </summary>
    public class InputBuffer : MonoBehaviour
    {
        [SerializeField] private RhythmJudgement judgement;
        [SerializeField] private float bufferWindowSeconds = 0.150f;

        public event Action<RhythmGrade> OnResolved;

        private bool buffered;
        private double bufferedTime;
        private BeatClock subscribedClock;

        private void Awake()
        {
            if (judgement == null)
                judgement = GetComponent<RhythmJudgement>();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (subscribedClock != null)
            {
                subscribedClock.OnBeat -= HandleBeat;
                subscribedClock = null;
            }
        }

        public void RegisterPress()
        {
            double now = AudioSettings.dspTime;
            if (BeatClock.Instance != null)
            {
                double dt = BeatClock.Instance.TimeToNearestBeat(now);
                RhythmGrade grade = System.Math.Abs(dt) <= bufferWindowSeconds && judgement != null
                    ? judgement.Judge(dt)
                    : RhythmGrade.Miss;
                OnResolved?.Invoke(grade);
                buffered = false;
                return;
            }

            buffered = true;
            bufferedTime = now;
        }

        public void Configure(RhythmJudgement rhythmJudgement)
        {
            judgement = rhythmJudgement;
        }

        private void TrySubscribe()
        {
            if (subscribedClock != null || BeatClock.Instance == null) return;
            subscribedClock = BeatClock.Instance;
            subscribedClock.OnBeat += HandleBeat;
        }

        private void HandleBeat(int beatIndex, double beatTime)
        {
            if (!buffered) return;
            double dt = bufferedTime - beatTime; // signed delta to the resolved beat
            if (System.Math.Abs(dt) <= bufferWindowSeconds)
            {
                var grade = judgement != null ? judgement.Judge(dt) : RhythmGrade.Miss;
                OnResolved?.Invoke(grade);
                buffered = false;
            }
            else
            {
                // Press too far from beat, treat as miss and clear
                OnResolved?.Invoke(RhythmGrade.Miss);
                buffered = false;
            }
        }
    }
}
