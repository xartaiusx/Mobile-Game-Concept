using UnityEngine;
using UnityEngine.UI;
using Game.Rhythm;

namespace Game.UI
{
    /// <summary>
    /// Simple UI widget that shows a moving marker across a bar each beat.
    /// Assign a RectTransform for the fill and a Text for judgement feedback.
    /// </summary>
    public class BeatBarUI : MonoBehaviour
    {
        [SerializeField] private RectTransform marker;
        [SerializeField] private RectTransform bar;
        [SerializeField] private Text feedbackText;

        private void Update()
        {
            if (BeatClock.Instance == null || marker == null || bar == null) return;
            double phase = BeatClock.Instance.BeatPhase(AudioSettings.dspTime);
            float x = Mathf.Lerp(0f, bar.rect.width, (float)phase);
            var pos = marker.anchoredPosition;
            pos.x = x - bar.rect.width * 0.5f;
            marker.anchoredPosition = pos;
        }

        public void ShowFeedback(RhythmGrade grade)
        {
            if (feedbackText == null) return;
            feedbackText.text = grade.ToString();
        }
    }
}
