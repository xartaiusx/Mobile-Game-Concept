#if UNITY_EDITOR
using Game.Editor;
using Game.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class VisualAssetSetupTests
{
    [Test]
    public void VisualAssetSetupValidationPassesBeforeThirdPartyImports()
    {
        Assert.IsTrue(VisualAssetSetupValidator.ValidateVisualAssetSetup(false));
    }

    [Test]
    public void VisualPrefabsHaveNoMissingScriptsOrMaterials()
    {
        string[] paths =
        {
            "Assets/Art/Prefabs/WarriorVisual.prefab",
            "Assets/Art/Prefabs/BasicEnemyVisual.prefab",
            "Assets/Art/Prefabs/BossVisual.prefab"
        };

        for (int i = 0; i < paths.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            Assert.IsNotNull(prefab, paths[i]);
            Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), paths[i] + " has missing scripts.");
            Assert.IsNotNull(prefab.GetComponent<VisualAttachmentRoot>(), paths[i] + " missing visual binder.");

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < renderers.Length; j++)
            {
                Material[] materials = renderers[j].sharedMaterials;
                for (int k = 0; k < materials.Length; k++)
                    Assert.IsNotNull(materials[k], paths[i] + " has a missing material on " + renderers[j].name);
            }
        }
    }

    [Test]
    public void KeyGameplayPrefabsHaveVisualRootsAndGameplayComponents()
    {
        AssertGameplayPrefab("Assets/Prefabs/Player/FighterPlayer.prefab", typeof(Game.Core.BaseCharacter));
        AssertGameplayPrefab("Assets/Prefabs/Enemies/MeleeEnemy.prefab", typeof(Game.Core.BaseEnemy));
        AssertGameplayPrefab("Assets/Prefabs/Enemies/RangedEnemy.prefab", typeof(Game.Core.BaseEnemy));
        AssertGameplayPrefab("Assets/Prefabs/Enemies/BossEnemy.prefab", typeof(Game.Core.BaseEnemy));
    }

    [Test]
    public void VisualAttachmentRootMethodsAreNullSafe()
    {
        GameObject go = new GameObject("visual-binder-test");
        try
        {
            var binder = go.AddComponent<VisualAttachmentRoot>();
            Assert.DoesNotThrow(() => binder.SetModel(null));
            Assert.DoesNotThrow(() => binder.PlayState(null));
            Assert.DoesNotThrow(() => binder.PlayState("Idle"));
            Assert.DoesNotThrow(() => binder.SetTrigger(null));
            Assert.DoesNotThrow(() => binder.SetTrigger("Attack"));
            Assert.DoesNotThrow(() => binder.ClearModel());
            Assert.IsNotNull(binder.VisualRoot);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void RuntimeVisualAssemblyDoesNotDependOnUnityEditor()
    {
        Assert.IsNull(typeof(VisualAttachmentRoot).Assembly.GetReferencedAssemblies()
            .FirstOrDefaultName("UnityEditor"));
    }

    private static void AssertGameplayPrefab(string path, System.Type requiredComponent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Assert.IsNotNull(prefab, path);
        Assert.IsNotNull(prefab.GetComponent(requiredComponent), path + " missing gameplay component.");
        Assert.IsNotNull(prefab.transform.Find("VisualRoot"), path + " missing VisualRoot.");
        Assert.IsNotNull(prefab.GetComponent<VisualAttachmentRoot>(), path + " missing VisualAttachmentRoot.");
        Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), path + " has missing scripts.");
    }
}
#endif
