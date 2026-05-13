#if UNITY_EDITOR
using System;
using Game.Combat;
using Game.Editor;
using Game.Rhythm;
using Game.Systems;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ProjectAuditTests
{
    [Test]
    public void StartupSceneExistsAndIsEnabledInBuildSettings()
    {
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(VerticalSliceStartup.ScenePath));
        Assert.IsEmpty(VerticalSliceStartup.GetStartupValidationFailures());

        bool enabled = false;
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path == VerticalSliceStartup.ScenePath)
                enabled = scene.enabled;
        }

        Assert.IsTrue(enabled, VerticalSliceStartup.ScenePath + " must be enabled in Build Settings.");
    }

    [Test]
    public void RuntimeAssemblyDoesNotReferenceUnityEditor()
    {
        AssertAssemblyDoesNotReferenceUnityEditor(typeof(RhythmConfig).Assembly);
        AssertAssemblyDoesNotReferenceUnityEditor(typeof(TelemetryAnalysis).Assembly);
    }

    [Test]
    public void AllProjectPrefabsHaveNoMissingScripts()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        Assert.IsNotEmpty(prefabGuids);

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, path);

            foreach (Transform transform in prefab.GetComponentsInChildren<Transform>(true))
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                Assert.AreEqual(0, missing, path + " contains missing scripts on " + transform.name);
            }
        }
    }

    [Test]
    public void RenderableProjectPrefabsHaveNoMissingMaterialSlots()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        Assert.IsNotEmpty(prefabGuids);

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, path);

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                    Assert.IsNotNull(materials[j], path + " has a missing material on " + renderers[i].name);
            }
        }
    }

    [Test]
    public void DefaultTuningAssetsHaveSaneRanges()
    {
        RhythmConfig config = AssetDatabase.LoadAssetAtPath<RhythmConfig>("Assets/ScriptableObjects/Rhythm/DefaultRhythmConfig.asset");
        Assert.IsNotNull(config);
        Assert.Greater(config.bpm, 0f);
        Assert.Greater(config.perfectWindow, 0f);
        Assert.GreaterOrEqual(config.goodWindow, config.perfectWindow);
        Assert.GreaterOrEqual(config.earlyInputBiasSeconds, 0f);
        Assert.GreaterOrEqual(config.lateInputBiasSeconds, 0f);
        Assert.GreaterOrEqual(config.maxSpeedMultiplier, 1f);
        Assert.GreaterOrEqual(config.perfectDamageMultiplier, config.goodDamageMultiplier);
        Assert.GreaterOrEqual(config.goodDamageMultiplier, config.missDamageMultiplier);
        Assert.GreaterOrEqual(config.perfectScore, config.goodScore);
        Assert.GreaterOrEqual(config.goodScore, config.missScore);

        ComboProfile combo = AssetDatabase.LoadAssetAtPath<ComboProfile>("Assets/ScriptableObjects/Combat/DefaultComboProfile.asset");
        Assert.IsNotNull(combo);
        Assert.IsNotEmpty(combo.steps);
        Assert.Greater(combo.comboTimeout, 0f);
        for (int i = 0; i < combo.steps.Length; i++)
        {
            Assert.Greater(combo.steps[i].baseDamage, 0, "Combo step damage must be positive.");
            Assert.GreaterOrEqual(combo.steps[i].cooldown, 0f, "Combo cooldown cannot be negative.");
        }

        AttackTimingData timing = AssetDatabase.LoadAssetAtPath<AttackTimingData>("Assets/ScriptableObjects/Combat/DefaultAttackTiming.asset");
        Assert.IsNotNull(timing);
        Assert.GreaterOrEqual(timing.windupSeconds, 0f);
        Assert.GreaterOrEqual(timing.activeSeconds, 0f);
        Assert.GreaterOrEqual(timing.recoverySeconds, 0f);
    }

    [Test]
    public void TelemetryAnalyzerValidationCoversEmptyValidAndMalformedSamples()
    {
        Assert.IsTrue(TelemetryAnalysisWindow.ValidateAnalyzer());
    }

    [Test]
    public void AlphaPlaytestValidationCoversSceneDocsVisualsAndArchive()
    {
        Assert.IsTrue(AlphaPlaytestValidator.ValidateAlphaPlaytest(false));
    }

    [Test]
    public void VerticalSliceSceneContainsNoMissingScriptsInEditMode()
    {
        string previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        try
        {
            EditorSceneManager.OpenScene(VerticalSliceStartup.ScenePath, OpenSceneMode.Single);
            foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                AssertNoMissingScriptsRecursive(go);
        }
        finally
        {
            if (!string.IsNullOrEmpty(previousScene))
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }
    }

    private static void AssertNoMissingScriptsRecursive(GameObject go)
    {
        Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go), go.name + " has missing scripts.");
        foreach (Transform child in go.transform)
            AssertNoMissingScriptsRecursive(child.gameObject);
    }

    private static void AssertAssemblyDoesNotReferenceUnityEditor(System.Reflection.Assembly assembly)
    {
        foreach (System.Reflection.AssemblyName reference in assembly.GetReferencedAssemblies())
            Assert.AreNotEqual("UnityEditor", reference.Name, assembly.GetName().Name + " references UnityEditor.");
    }
}
#endif
