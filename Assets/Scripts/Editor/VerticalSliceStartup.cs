#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor
{
    [InitializeOnLoad]
    public static class VerticalSliceStartup
    {
        public const string ScenePath = "Assets/Scenes/VerticalSlice.unity";

        static VerticalSliceStartup()
        {
            EditorApplication.delayCall += EnsureStartupSceneConfigured;
        }

        [MenuItem("Game/Vertical Slice/Validate Startup Scene")]
        public static bool ValidateStartupScene()
        {
            EnsureStartupSceneConfigured();
            List<string> failures = GetStartupValidationFailures();
            if (failures.Count == 0)
            {
                Debug.Log("Vertical slice startup validation passed.");
                return true;
            }

            Debug.LogError("Vertical slice startup validation failed:\n" + string.Join("\n", failures));
            return false;
        }

        public static void EnsureStartupSceneConfigured()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset == null)
            {
                Debug.LogError("Vertical slice startup scene is missing at " + ScenePath + ".");
                return;
            }

            if (!IsUnityTestRun() && EditorSceneManager.playModeStartScene != sceneAsset)
                EditorSceneManager.playModeStartScene = sceneAsset;

            EnsureSceneInBuildSettings(ScenePath);
        }

        public static List<string> GetStartupValidationFailures()
        {
            var failures = new List<string>();

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset == null)
            {
                failures.Add("Missing startup scene: " + ScenePath);
                return failures;
            }

            if (!IsUnityTestRun())
            {
                string configuredStartScene = AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene);
                if (!string.Equals(configuredStartScene, ScenePath, StringComparison.Ordinal))
                    failures.Add("Play Mode start scene is '" + configuredStartScene + "', expected '" + ScenePath + "'.");
            }

            if (!IsSceneEnabledInBuildSettings(ScenePath))
                failures.Add("Startup scene is not enabled in Build Settings: " + ScenePath);

            return failures;
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!string.Equals(scenes[i].path, scenePath, StringComparison.Ordinal))
                    continue;

                if (!scenes[i].enabled)
                {
                    scenes[i].enabled = true;
                    EditorBuildSettings.scenes = scenes;
                }

                return;
            }

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, updated, scenes.Length);
            updated[updated.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }

        private static bool IsSceneEnabledInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (string.Equals(scenes[i].path, scenePath, StringComparison.Ordinal))
                    return scenes[i].enabled;
            }

            return false;
        }

        private static bool IsUnityTestRun()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-runTests", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
#endif
