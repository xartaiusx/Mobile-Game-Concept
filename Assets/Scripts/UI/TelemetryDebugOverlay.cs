using System.Text;
using Game.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TelemetryDebugOverlay : MonoBehaviour
    {
        [SerializeField] private bool showInDevelopmentBuilds = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        [SerializeField] private KeyCode startRunKey = KeyCode.F4;
        [SerializeField] private KeyCode writeRunKey = KeyCode.F5;
        [SerializeField] private KeyCode clearRunKey = KeyCode.F6;
        [SerializeField] private Text telemetryText;
        [SerializeField] private Button mobileToggleButton;
        [SerializeField] private float refreshInterval = 0.25f;

        private readonly StringBuilder builder = new StringBuilder(256);
        private float refreshTimer;
        private bool visible;

        private void Awake()
        {
            EnsureUi();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (mobileToggleButton != null)
                mobileToggleButton.onClick.AddListener(Toggle);
        }

        private void OnDisable()
        {
            if (mobileToggleButton != null)
                mobileToggleButton.onClick.RemoveListener(Toggle);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                Toggle();

            TelemetryManager telemetry = TelemetryManager.Instance;
            if (telemetry != null)
            {
                if (Input.GetKeyDown(startRunKey))
                    telemetry.StartNewRun();
                if (Input.GetKeyDown(writeRunKey))
                    telemetry.WriteRunSummary();
                if (Input.GetKeyDown(clearRunKey))
                    telemetry.ClearCurrentRunData();
            }

            if (!visible || telemetryText == null)
                return;

            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer > 0f)
                return;

            refreshTimer = Mathf.Max(0.05f, refreshInterval);
            RefreshText();
        }

        public void Toggle()
        {
            SetVisible(!visible);
        }

        private void SetVisible(bool value)
        {
            visible = value && showInDevelopmentBuilds;
            if (telemetryText != null)
                telemetryText.enabled = visible;
            RefreshText();
        }

        private void RefreshText()
        {
            if (telemetryText == null)
                return;

            TelemetryManager telemetry = TelemetryManager.Instance;
            if (telemetry == null)
            {
                telemetryText.text = "Telemetry unavailable";
                return;
            }

            TelemetrySnapshot snapshot = telemetry.Snapshot;
            builder.Length = 0;
            builder.Append("Telemetry\nHits P/G/M: ");
            builder.Append(snapshot.perfectHitCount).Append('/').Append(snapshot.goodHitCount).Append('/').Append(snapshot.missHitCount);
            builder.Append("\nDodges P/G/M: ");
            builder.Append(snapshot.perfectDodgeCount).Append('/').Append(snapshot.goodDodgeCount).Append('/').Append(snapshot.missDodgeCount);
            builder.Append("\nCombo: ").Append(telemetry.CurrentComboLength).Append("  Max: ").Append(snapshot.maxCombo);
            builder.Append("\nTiming Avg: ").Append(telemetry.AverageTimingOffset >= 0f ? "+" : string.Empty).Append(telemetry.AverageTimingOffset.ToString("0.000")).Append("s");
            builder.Append("\nScore/min: ").Append(telemetry.ScorePerMinute.ToString("0"));
            builder.Append("\nBatch: ").Append(string.IsNullOrEmpty(telemetry.BatchLabel) ? "(none)" : telemetry.BatchLabel);
            builder.Append("\nF4 start  F5 write  F6 clear");
            telemetryText.text = builder.ToString();
        }

        private void EnsureUi()
        {
            if (telemetryText != null)
                return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("TelemetryOverlayCanvas");
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject textObject = new GameObject("TelemetryDebugText");
            textObject.transform.SetParent(canvas.transform, false);
            telemetryText = textObject.AddComponent<Text>();
            telemetryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            telemetryText.fontSize = 13;
            telemetryText.alignment = TextAnchor.UpperLeft;
            telemetryText.color = new Color(0.9f, 1f, 0.9f, 0.95f);

            RectTransform rect = telemetryText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(12f, -12f);
            rect.sizeDelta = new Vector2(320f, 156f);
        }
    }
}
