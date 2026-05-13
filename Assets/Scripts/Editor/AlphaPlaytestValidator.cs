#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using Game.Systems;
using Game.UI;
using Game.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor
{
    public static class AlphaPlaytestValidator
    {
        [MenuItem("Game/Vertical Slice/Validate Alpha Playtest")]
        public static bool ValidateAlphaPlaytestMenu()
        {
            return ValidateAlphaPlaytest(true);
        }

        public static bool ValidateAlphaPlaytest(bool logResult)
        {
            List<string> failures = GetValidationFailures();
            if (failures.Count == 0)
            {
                if (logResult)
                    Debug.Log("Alpha playtest validation passed.");
                return true;
            }

            if (logResult)
                Debug.LogError("Alpha playtest validation failed:\n" + string.Join("\n", failures));
            return false;
        }

        public static List<string> GetValidationFailures()
        {
            var failures = new List<string>();
            VerticalSliceStartup.EnsureStartupSceneConfigured();
            failures.AddRange(VerticalSliceStartup.GetStartupValidationFailures());
            failures.AddRange(VisualAssetSetupValidator.GetValidationFailures());

            ArchiveValidationReport archive = AssetArchiveValidator.CreateReport();
            failures.AddRange(archive.failures);

            ValidateScene(failures);
            ValidateDocs(failures);
            ValidateRuntimeAssembly(failures);
            return failures;
        }

        private static void ValidateScene(List<string> failures)
        {
            string previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            try
            {
                EditorSceneManager.OpenScene(VerticalSliceStartup.ScenePath, OpenSceneMode.Single);

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length != 1)
                {
                    failures.Add("Alpha scene must contain exactly one active Player-tagged object.");
                    return;
                }

                GameObject player = players[0];
                RequireComponent<BaseCharacter>(player, failures);
                RequireComponent<PlayerController>(player, failures);
                RequireComponent<ComboSystem>(player, failures);
                RequireComponent<DodgeController>(player, failures);
                RequireComponent<InventorySystem>(player, failures);
                RequireComponent<VisualAttachmentRoot>(player, failures);
                RequireRenderableVisual(player, "WarriorVisual", failures);

                if (Camera.main == null)
                    failures.Add("Alpha scene is missing an enabled MainCamera.");
                if (UnityEngine.Object.FindAnyObjectByType<Canvas>() == null)
                    failures.Add("Alpha scene is missing a HUD Canvas.");
                if (UnityEngine.Object.FindAnyObjectByType<BeatClock>() == null)
                    failures.Add("Alpha scene is missing BeatClock.");
                if (UnityEngine.Object.FindAnyObjectByType<RhythmJudgement>() == null)
                    failures.Add("Alpha scene is missing RhythmJudgement.");
                if (UnityEngine.Object.FindAnyObjectByType<ScoreSystem>() == null)
                    failures.Add("Alpha scene is missing ScoreSystem.");
                if (UnityEngine.Object.FindAnyObjectByType<TelemetryManager>() == null)
                    failures.Add("Alpha scene is missing TelemetryManager.");
                if (UnityEngine.Object.FindAnyObjectByType<BeatBarUI>() == null)
                    failures.Add("Alpha scene is missing BeatBarUI.");
                if (UnityEngine.Object.FindAnyObjectByType<VerticalSliceHud>() == null)
                    failures.Add("Alpha scene is missing VerticalSliceHud.");

                ArenaController arena = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
                if (arena == null)
                {
                    failures.Add("Alpha scene is missing ArenaController.");
                }
                else
                {
                    var serializedArena = new SerializedObject(arena);
                    SerializedProperty initialEnemies = serializedArena.FindProperty("initialWaveEnemies");
                    SerializedProperty restartKey = serializedArena.FindProperty("restartKey");
                    if (arena.SessionWaveLimit < 1)
                        failures.Add("ArenaController must have waveCount >= 1 for the short alpha session-complete path.");
                    if (initialEnemies == null || initialEnemies.arraySize == 0)
                        failures.Add("ArenaController must reference at least one initial wave enemy.");
                    if (restartKey == null || restartKey.intValue != (int)KeyCode.R)
                        failures.Add("ArenaController restart key must be R for alpha retry.");
                }

                BaseEnemy[] activeEnemies = UnityEngine.Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                int activeWaveEnemies = 0;
                for (int i = 0; i < activeEnemies.Length; i++)
                {
                    if (activeEnemies[i] is BossEnemy)
                        continue;

                    activeWaveEnemies++;
                    RequireRenderableVisual(activeEnemies[i].gameObject, "BasicEnemyVisual", failures);
                }

                if (activeWaveEnemies == 0)
                    failures.Add("Alpha scene must start with at least one active non-boss enemy.");

                BossEnemy boss = UnityEngine.Object.FindAnyObjectByType<BossEnemy>(FindObjectsInactive.Include);
                if (boss == null)
                    failures.Add("Alpha scene is missing the boss placeholder.");
                else
                    RequireRenderableVisual(boss.gameObject, "BossVisual", failures);

                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                    ValidateNoMissingScriptsRecursive(root, failures);

                if (!Mathf.Approximately(Time.timeScale, 1f))
                    failures.Add("Time.timeScale must be 1 for alpha startup.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousScene))
                    EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
        }

        private static void ValidateDocs(List<string> failures)
        {
            RequireDocText("README.md", "Alpha Playtest Checklist", failures);
            RequireDocText("README.md", "alpha_warrior_batch_001", failures);
            RequireDocText("DEVELOPMENT.md", "Alpha Playtest Checklist", failures);
            RequireDocText("DEVELOPMENT.md", "alpha_warrior_batch_001", failures);
        }

        private static void ValidateRuntimeAssembly(List<string> failures)
        {
            Assembly runtimeAssembly = typeof(TelemetryManager).Assembly;
            foreach (AssemblyName reference in runtimeAssembly.GetReferencedAssemblies())
            {
                if (reference.Name == "UnityEditor")
                    failures.Add(runtimeAssembly.GetName().Name + " references UnityEditor.");
            }
        }

        private static void RequireComponent<T>(GameObject go, List<string> failures) where T : Component
        {
            if (go.GetComponent<T>() == null)
                failures.Add(go.name + " is missing " + typeof(T).Name + ".");
        }

        private static void RequireRenderableVisual(GameObject go, string expectedChildName, List<string> failures)
        {
            if (go == null)
                return;

            VisualAttachmentRoot binder = go.GetComponent<VisualAttachmentRoot>();
            if (binder == null)
            {
                failures.Add(go.name + " is missing VisualAttachmentRoot.");
                return;
            }

            binder.RefreshReferences();
            Transform visualRoot = binder.VisualRoot;
            if (visualRoot == null)
            {
                failures.Add(go.name + " is missing VisualRoot.");
                return;
            }

            if (visualRoot.Find(expectedChildName) == null)
                failures.Add(go.name + " is missing visual child " + expectedChildName + ".");

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                failures.Add(go.name + " visual has no renderers.");

            bool enabledRenderer = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer.enabled)
                    enabledRenderer = true;
                if (renderer.bounds.size.sqrMagnitude <= 0.0001f)
                    failures.Add(renderer.name + " has zero-size visual bounds.");

                Material[] materials = renderer.sharedMaterials;
                if (materials.Length == 0)
                    failures.Add(renderer.name + " has no material slots.");
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] == null)
                        failures.Add(renderer.name + " has a missing material slot.");
                }
            }

            if (!enabledRenderer)
                failures.Add(go.name + " visual has no enabled renderer.");
        }

        private static void ValidateNoMissingScriptsRecursive(GameObject go, List<string> failures)
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
                failures.Add(go.name + " has missing scripts.");
            foreach (Transform child in go.transform)
                ValidateNoMissingScriptsRecursive(child.gameObject, failures);
        }

        private static void RequireDocText(string path, string requiredText, List<string> failures)
        {
            if (!File.Exists(path))
            {
                failures.Add("Missing documentation file: " + path);
                return;
            }

            if (!File.ReadAllText(path).Contains(requiredText, StringComparison.Ordinal))
                failures.Add(path + " is missing required alpha playtest text: " + requiredText);
        }
    }
}
#endif
