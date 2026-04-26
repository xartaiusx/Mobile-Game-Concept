#if UNITY_EDITOR
using System;
using System.IO;
using Game.Systems;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class TelemetryAnalysisWindow : EditorWindow
    {
        private string telemetryDirectory;
        private string selectedBatchLabel;
        private string runNotes;
        private bool analyzeAllBatches = true;
        private TelemetryAnalysisSummary summary;
        private Vector2 scroll;

        [MenuItem("Game/Telemetry/Analyze Runs")]
        public static void Open()
        {
            GetWindow<TelemetryAnalysisWindow>("Telemetry Analysis");
        }

        [MenuItem("Game/Telemetry/Validate Analyzer")]
        public static void ValidateAnalyzerMenu()
        {
            if (ValidateAnalyzer())
                Debug.Log("Telemetry analyzer validation passed.");
            else
                Debug.LogError("Telemetry analyzer validation failed.");
        }

        public static bool ValidateAnalyzer()
        {
            string emptyDirectory = Path.Combine(Path.GetTempPath(), "warrior-telemetry-empty-" + Guid.NewGuid().ToString("N"));
            string sampleDirectory = Path.Combine(Path.GetTempPath(), "warrior-telemetry-sample-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(emptyDirectory);
                TelemetryAnalysisSummary empty = TelemetryAnalysis.AnalyzeDirectory(emptyDirectory);
                if (empty.fileCount != 0 || empty.validRunCount != 0)
                    return false;

                Directory.CreateDirectory(sampleDirectory);
                File.WriteAllText(Path.Combine(sampleDirectory, "run_sample.json"), SampleTelemetryJson());
                File.WriteAllText(Path.Combine(sampleDirectory, "run_broken.json"), "{broken telemetry");
                string batchDirectory = Path.Combine(sampleDirectory, "warrior_batch_001");
                Directory.CreateDirectory(batchDirectory);
                File.WriteAllText(Path.Combine(batchDirectory, "run_sample_batch.json"), SampleTelemetryJson("warrior_batch_001"));
                TelemetryAnalysisSummary sample = TelemetryAnalysis.AnalyzeDirectory(sampleDirectory, true);
                return sample.fileCount == 3
                    && sample.validRunCount == 2
                    && sample.shortSessionCount == 0
                    && sample.parseErrors.Count == 1
                    && sample.batchLabels.Contains("warrior_batch_001");
            }
            catch (Exception ex)
            {
                Debug.LogError("Telemetry analyzer validation exception: " + ex.Message);
                return false;
            }
            finally
            {
                TryDeleteDirectory(emptyDirectory);
                TryDeleteDirectory(sampleDirectory);
            }
        }

        private void OnEnable()
        {
            telemetryDirectory = Path.Combine(Application.persistentDataPath, "Telemetry");
            selectedBatchLabel = selectedBatchLabel ?? string.Empty;
            runNotes = runNotes ?? string.Empty;
            Analyze();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Telemetry Folder", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(telemetryDirectory, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            selectedBatchLabel = EditorGUILayout.TextField("Batch Label", selectedBatchLabel);
            runNotes = EditorGUILayout.TextField("Run Notes", runNotes);
            analyzeAllBatches = EditorGUILayout.Toggle("Analyze All Batches", analyzeAllBatches);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Analyze"))
                Analyze();
            if (GUILayout.Button("Analyze Batch"))
                AnalyzeBatch();
            if (GUILayout.Button("Choose Folder"))
                ChooseTelemetryFolder();
            if (GUILayout.Button("Open Telemetry Folder"))
                OpenTelemetryFolder();
            if (GUILayout.Button("Copy Summary to Clipboard"))
                GUIUtility.systemCopyBuffer = summary != null ? summary.ToHumanReadableString() : string.Empty;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            DrawCaptureControls();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSummary();
            EditorGUILayout.EndScrollView();
        }

        private void Analyze()
        {
            summary = TelemetryAnalysis.AnalyzeDirectory(telemetryDirectory, analyzeAllBatches);
        }

        private void AnalyzeBatch()
        {
            string safeLabel = TelemetryManager.SanitizeBatchLabel(selectedBatchLabel);
            string directory = string.IsNullOrEmpty(safeLabel) ? telemetryDirectory : Path.Combine(telemetryDirectory, safeLabel);
            summary = TelemetryAnalysis.AnalyzeDirectory(directory, false);
        }

        private void ChooseTelemetryFolder()
        {
            string selected = EditorUtility.OpenFolderPanel("Choose Telemetry Folder", telemetryDirectory, string.Empty);
            if (!string.IsNullOrEmpty(selected))
            {
                telemetryDirectory = selected;
                Analyze();
            }
        }

        private void OpenTelemetryFolder()
        {
            if (!Directory.Exists(telemetryDirectory))
                Directory.CreateDirectory(telemetryDirectory);

            EditorUtility.RevealInFinder(telemetryDirectory);
        }

        private void DrawCaptureControls()
        {
            EditorGUILayout.LabelField("Play Mode Capture", EditorStyles.boldLabel);
            TelemetryManager telemetry = Application.isPlaying ? TelemetryManager.Instance : null;
            if (telemetry == null)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to control the active telemetry run.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Active Batch", string.IsNullOrEmpty(telemetry.BatchLabel) ? "(none)" : telemetry.BatchLabel);
            EditorGUILayout.LabelField("Current Run Folder", telemetry.GetCurrentRunDirectory());
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Batch / Notes"))
            {
                telemetry.SetBatchLabel(selectedBatchLabel);
                telemetry.SetRunNotes(runNotes);
            }
            if (GUILayout.Button("Start New Run"))
                telemetry.StartNewRun();
            if (GUILayout.Button("End / Write Run"))
                telemetry.WriteRunSummary();
            if (GUILayout.Button("Clear Current Run"))
                telemetry.ClearCurrentRunData();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummary()
        {
            if (summary == null)
            {
                EditorGUILayout.HelpBox("No telemetry analysis has run yet.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Files Found", summary.fileCount.ToString());
            EditorGUILayout.LabelField("Valid Runs", summary.validRunCount.ToString());
            EditorGUILayout.LabelField("Normal Runs", summary.normalRunCount.ToString());
            EditorGUILayout.LabelField("Short / Smoke Sessions", summary.shortSessionCount.ToString());
            EditorGUILayout.LabelField("Parse Errors", summary.parseErrors.Count.ToString());
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Aggregate Metrics", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(summary.ToHumanReadableString(), GUILayout.MinHeight(180f));

            if (summary.parseErrors.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Parse Errors", EditorStyles.boldLabel);
                for (int i = 0; i < summary.parseErrors.Count; i++)
                {
                    TelemetryParseError error = summary.parseErrors[i];
                    EditorGUILayout.HelpBox(Path.GetFileName(error.sourcePath) + ": " + error.message, MessageType.Warning);
                }
            }

            if (!Directory.Exists(telemetryDirectory))
                EditorGUILayout.HelpBox("Telemetry folder does not exist yet. Play a run or call WriteRunSummary() to create local JSON.", MessageType.Info);
        }

        private static string SampleTelemetryJson(string batchLabel = "")
        {
            return "{"
                + "\"schemaVersion\":3,"
                + "\"runStartedAtUtc\":\"2026-04-26T12:00:00.0000000Z\","
                + "\"runEndedAtUtc\":\"2026-04-26T12:01:00.0000000Z\","
                + "\"runDurationSeconds\":60,"
                + "\"sceneName\":\"VerticalSlice\","
                + "\"playerClass\":\"Warrior\","
                + "\"batchLabel\":\"" + batchLabel + "\","
                + "\"runNotes\":\"generated analyzer validation sample\","
                + "\"perfectHitCount\":8,"
                + "\"goodHitCount\":14,"
                + "\"missHitCount\":6,"
                + "\"perfectDodgeCount\":2,"
                + "\"goodDodgeCount\":5,"
                + "\"missDodgeCount\":3,"
                + "\"timingOffsets\":[-0.02,0.01,0.03,-0.01],"
                + "\"averageTimingOffset\":0.0025,"
                + "\"totalSurvivalTime\":60,"
                + "\"enemyKillCount\":9,"
                + "\"maxCombo\":5,"
                + "\"averageComboLength\":4,"
                + "\"totalScore\":1200,"
                + "\"scorePerMinute\":1200"
                + "}";
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                // Best-effort cleanup for editor validation temp folders.
            }
        }
    }
}
#endif
