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
        [SerializeField] private RectTransform perfectWindow;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Text speedText;
        [SerializeField] private float barWidth = 320f;
        [SerializeField] private float sliderWidth = 8f;
        [SerializeField] private float minimumPerfectZoneWidth = 18f;
        [SerializeField] private Color barColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color perfectZoneColor = new Color(0.15f, 1f, 0.35f, 0.7f);
        [SerializeField] private Color sliderColor = Color.white;

        public float PerfectZoneNormalized { get; private set; }

        private void Update()
        {
            if (BeatClock.Instance == null || marker == null || bar == null) return;
            ApplyStaticSizing();
            double phase = BeatClock.Instance.GetCenteredBeatBarPhase();
            float x = Mathf.Lerp(0f, bar.rect.width, (float)phase);
            var pos = marker.anchoredPosition;
            pos.x = x - bar.rect.width * 0.5f;
            marker.anchoredPosition = pos;

            if (speedText != null)
                speedText.text = "BPM " + BeatClock.Instance.GetEffectiveBpm().ToString("0");
        }

        public void ShowFeedback(RhythmGrade grade)
        {
            if (feedbackText == null) return;
            feedbackText.text = grade.ToString();
        }

        private void Awake()
        {
            if (perfectWindow == null && bar != null)
            {
                Transform child = bar.Find("PerfectWindow");
                if (child != null)
                    perfectWindow = child as RectTransform;
            }
        }

        private void ApplyStaticSizing()
        {
            if (bar != null)
            {
                bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barWidth);
                Image image = bar.GetComponent<Image>();
                if (image != null)
                    image.color = barColor;
            }

            if (marker != null)
            {
                marker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sliderWidth);
                Image image = marker.GetComponent<Image>();
                if (image != null)
                    image.color = sliderColor;
            }

            if (perfectWindow != null && BeatClock.Instance != null && bar != null)
            {
                PerfectZoneNormalized = BeatClock.Instance.GetPerfectWindowNormalized();
                float width = Mathf.Max(minimumPerfectZoneWidth, bar.rect.width * PerfectZoneNormalized);
                perfectWindow.anchorMin = new Vector2(0.5f, 0f);
                perfectWindow.anchorMax = new Vector2(0.5f, 1f);
                perfectWindow.pivot = new Vector2(0.5f, 0.5f);
                perfectWindow.anchoredPosition = Vector2.zero;
                perfectWindow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

                Image image = perfectWindow.GetComponent<Image>();
                if (image != null)
                    image.color = perfectZoneColor;
            }
        }
    }
}
