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

        private void OnEnable()
        {
            if (BeatClock.Instance != null)
                BeatClock.Instance.OnBeat += HandleBeat;
        }

        private void OnDisable()
        {
            if (BeatClock.Instance != null)
                BeatClock.Instance.OnBeat -= HandleBeat;
        }

        public void RegisterPress()
        {
            double now = AudioSettings.dspTime;
            // Overwrite if newer press is closer to next beat
            buffered = true;
            bufferedTime = now;
        }

        private void HandleBeat(int beatIndex, double beatTime)
        {
            if (!buffered) return;
            double dt = bufferedTime - beatTime; // signed delta to the resolved beat
            if (System.Math.Abs(dt) <= bufferWindowSeconds)
            {
                var grade = judgement.Judge(dt);
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
