#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner;
using ApiTestStatus = UnityEditor.TestTools.TestRunner.Api.TestStatus;
using NUnitTestStatus = NUnit.Framework.Interfaces.TestStatus;

[assembly: TestRunCallback(typeof(Game.Editor.NUnitProjectTestSummaryCallback))]

namespace Game.Editor
{
    public static class ProjectTestRunner
    {
        private const string ResultsDirectory = "TestResults";
        private const string EditModeFileName = "editmode-summary.json";
        private const string PlayModeFileName = "playmode-summary.json";
        private const string CombinedFileName = "summary.txt";
        private const string RunningKey = "Game.Editor.ProjectTestRunner.Running";
        private const string CompletedKey = "Game.Editor.ProjectTestRunner.Completed";
        private const string ModeKey = "Game.Editor.ProjectTestRunner.Mode";

        static ProjectTestRunner()
        {
            EditorApplication.delayCall += RestoreCallbacksAfterReload;
        }

        public static void RunEditMode()
        {
            RunOrPrepare(TestMode.EditMode);
        }

        public static void RunPlayMode()
        {
            RunOrPrepare(TestMode.PlayMode);
        }

        private static void RunOrPrepare(TestMode mode)
        {
            if (IsUnityRunTestsInvocation())
            {
                PrepareCommandLineRun(mode);
                return;
            }

            PrepareCommandLineRun(mode);
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new SummaryCallbacks(mode));
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = mode
            }));
        }

        private static void PrepareCommandLineRun(TestMode mode)
        {
            Directory.CreateDirectory(ResultsDirectory);
            SessionState.SetBool(RunningKey, true);
            SessionState.SetBool(CompletedKey, false);
            SessionState.SetString(ModeKey, mode.ToString());
        }

        private static bool IsUnityRunTestsInvocation()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-runTests", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void RestoreCallbacksAfterReload()
        {
            if (!SessionState.GetBool(RunningKey, false) || SessionState.GetBool(CompletedKey, false))
                return;

            string modeName = SessionState.GetString(ModeKey, TestMode.EditMode.ToString());
            TestMode mode = modeName == TestMode.PlayMode.ToString() ? TestMode.PlayMode : TestMode.EditMode;
            TestRunnerApi.RegisterTestCallback(new SummaryCallbacks(mode));
        }

        public static void WriteSummaryFromNUnitResult(ITestResult result)
        {
            if (!SessionState.GetBool(RunningKey, false) || SessionState.GetBool(CompletedKey, false))
                return;

            if (result == null || result.Test == null || result.Test.Parent != null)
                return;

            string modeName = SessionState.GetString(ModeKey, TestMode.EditMode.ToString());
            TestMode mode = modeName == TestMode.PlayMode.ToString() ? TestMode.PlayMode : TestMode.EditMode;

            SessionState.SetBool(CompletedKey, true);
            SessionState.SetBool(RunningKey, false);

            TestSummary summary = TestSummary.FromNUnitResult(mode.ToString(), result);
            string path = Path.Combine(ResultsDirectory, mode == TestMode.EditMode ? EditModeFileName : PlayModeFileName);
            Directory.CreateDirectory(ResultsDirectory);
            File.WriteAllText(path, summary.ToJson());
            WriteCombinedSummary();
        }

        private sealed class SummaryCallbacks : ICallbacks
        {
            private readonly TestMode mode;
            private readonly TestSummary collectedSummary;

            public SummaryCallbacks(TestMode mode)
            {
                this.mode = mode;
                collectedSummary = new TestSummary { mode = mode.ToString() };
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                if (SessionState.GetBool(CompletedKey, false))
                    return;

                SessionState.SetBool(CompletedKey, true);
                SessionState.SetBool(RunningKey, false);
                TestSummary summary = collectedSummary.total > 0 ? collectedSummary : TestSummary.FromResult(mode.ToString(), result);
                string path = Path.Combine(ResultsDirectory, mode == TestMode.EditMode ? EditModeFileName : PlayModeFileName);
                File.WriteAllText(path, summary.ToJson());
                WriteCombinedSummary();
                AssetDatabase.Refresh();
                EditorApplication.Exit(summary.failed > 0 ? 1 : 0);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                collectedSummary.RecordFinishedTest(result);
            }
        }

        [Serializable]
        private sealed class TestSummary
        {
            public string mode;
            public int total;
            public int passed;
            public int failed;
            public int skipped;
            public int inconclusive;
            public List<FailedTest> failures = new List<FailedTest>();

            public static TestSummary FromResult(string mode, ITestResultAdaptor result)
            {
                var summary = new TestSummary
                {
                    mode = mode,
                    passed = result != null ? result.PassCount : 0,
                    failed = result != null ? result.FailCount : 0,
                    skipped = result != null ? result.SkipCount : 0,
                    inconclusive = result != null ? result.InconclusiveCount : 0
                };
                summary.total = summary.passed + summary.failed + summary.skipped + summary.inconclusive;
                CollectFailures(result, summary);
                return summary;
            }

            public static TestSummary FromNUnitResult(string mode, ITestResult result)
            {
                var summary = new TestSummary
                {
                    mode = mode,
                    passed = result != null ? result.PassCount : 0,
                    failed = result != null ? result.FailCount : 0,
                    skipped = result != null ? result.SkipCount : 0,
                    inconclusive = result != null ? result.InconclusiveCount : 0
                };
                summary.total = summary.passed + summary.failed + summary.skipped + summary.inconclusive;
                CollectNUnitFailures(result, summary);
                return summary;
            }

            public string ToJson()
            {
                return JsonUtility.ToJson(this, true);
            }

            private static void CollectFailures(ITestResultAdaptor result, TestSummary summary)
            {
                if (result == null)
                    return;

                if (result.HasChildren)
                {
                    foreach (ITestResultAdaptor child in result.Children)
                        CollectFailures(child, summary);
                    return;
                }

                if (result.TestStatus == ApiTestStatus.Failed)
                    summary.failures.Add(new FailedTest
                    {
                        name = result.Test != null ? result.Test.FullName : result.Name,
                        message = result.Message ?? string.Empty
                    });
            }

            private static void CollectNUnitFailures(ITestResult result, TestSummary summary)
            {
                if (result == null)
                    return;

                if (result.HasChildren)
                {
                    foreach (ITestResult child in result.Children)
                        CollectNUnitFailures(child, summary);
                    return;
                }

                if (result.ResultState != null && result.ResultState.Status == NUnitTestStatus.Failed)
                    summary.failures.Add(new FailedTest
                    {
                        name = result.Test != null ? result.Test.FullName : result.Name,
                        message = result.Message ?? string.Empty
                    });
            }

            public void RecordFinishedTest(ITestResultAdaptor result)
            {
                if (result == null || (result.Test != null && result.Test.IsSuite))
                    return;

                total++;
                if (result.TestStatus == ApiTestStatus.Passed)
                {
                    passed++;
                }
                else if (result.TestStatus == ApiTestStatus.Failed)
                {
                    failed++;
                    failures.Add(new FailedTest
                    {
                        name = result.Test != null ? result.Test.FullName : result.Name,
                        message = result.Message ?? string.Empty
                    });
                }
                else if (result.TestStatus == ApiTestStatus.Inconclusive)
                {
                    inconclusive++;
                }
                else
                {
                    skipped++;
                }
            }
        }

        [Serializable]
        private sealed class FailedTest
        {
            public string name;
            public string message;
        }

        private static void WriteCombinedSummary()
        {
            Directory.CreateDirectory(ResultsDirectory);
            TestSummary editMode = ReadSummary(Path.Combine(ResultsDirectory, EditModeFileName));
            TestSummary playMode = ReadSummary(Path.Combine(ResultsDirectory, PlayModeFileName));

            var builder = new StringBuilder();
            builder.AppendLine("Unity Test Summary");
            AppendSummary(builder, "EditMode", editMode);
            AppendSummary(builder, "PlayMode", playMode);

            int failed = (editMode != null ? editMode.failed : 0) + (playMode != null ? playMode.failed : 0);
            builder.AppendLine("overallFailed=" + failed);
            File.WriteAllText(Path.Combine(ResultsDirectory, CombinedFileName), builder.ToString());
        }

        private static TestSummary ReadSummary(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                return JsonUtility.FromJson<TestSummary>(File.ReadAllText(path));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void AppendSummary(StringBuilder builder, string label, TestSummary summary)
        {
            if (summary == null)
            {
                builder.AppendLine(label + ": pending");
                return;
            }

            builder.AppendLine(label + ": total=" + summary.total + " passed=" + summary.passed + " failed=" + summary.failed + " skipped=" + summary.skipped + " inconclusive=" + summary.inconclusive);
            for (int i = 0; i < summary.failures.Count; i++)
                builder.AppendLine(label + " failure: " + summary.failures[i].name + " :: " + summary.failures[i].message);
        }
    }

    public sealed class NUnitProjectTestSummaryCallback : ITestRunCallback
    {
        public void RunStarted(ITest testsToRun)
        {
        }

        public void RunFinished(ITestResult testResults)
        {
            ProjectTestRunner.WriteSummaryFromNUnitResult(testResults);
        }

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
        }
    }
}
#endif
