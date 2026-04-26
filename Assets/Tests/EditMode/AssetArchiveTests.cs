#if UNITY_EDITOR
using System.IO;
using Game.Editor;
using Game.Visuals;
using NUnit.Framework;
using UnityEditor;

public class AssetArchiveTests
{
    [Test]
    public void ArchiveStructureExists()
    {
        Assert.IsTrue(AssetDatabase.IsValidFolder(AssetArchiveValidator.ArchiveRoot));
        Assert.IsTrue(AssetDatabase.IsValidFolder(AssetArchiveValidator.ArchiveRoot + "/KayKit"));
        Assert.IsTrue(AssetDatabase.IsValidFolder(AssetArchiveValidator.ArchiveRoot + "/Kenney"));
    }

    [Test]
    public void ArchiveManifestExistsAndIsNonEmpty()
    {
        Assert.IsTrue(File.Exists(AssetArchiveValidator.ReadmePath), AssetArchiveValidator.ReadmePath);
        Assert.IsTrue(File.Exists(AssetArchiveValidator.ManifestPath), AssetArchiveValidator.ManifestPath);
        Assert.Greater(new FileInfo(AssetArchiveValidator.ManifestPath).Length, 0);
    }

    [Test]
    public void ActiveSceneAndPrefabsDoNotReferenceArchive()
    {
        AssertNoLiteralArchiveReference("Assets/Scenes/VerticalSlice.unity");
        AssertNoLiteralArchiveReference("Assets/Art/Prefabs/WarriorVisual.prefab");
        AssertNoLiteralArchiveReference("Assets/Art/Prefabs/BasicEnemyVisual.prefab");
        AssertNoLiteralArchiveReference("Assets/Art/Prefabs/BossVisual.prefab");
        AssertNoLiteralArchiveReference("Assets/Prefabs/Player/FighterPlayer.prefab");
        AssertNoLiteralArchiveReference("Assets/Prefabs/Enemies/MeleeEnemy.prefab");
        AssertNoLiteralArchiveReference("Assets/Prefabs/Enemies/RangedEnemy.prefab");
        AssertNoLiteralArchiveReference("Assets/Prefabs/Enemies/BossEnemy.prefab");
    }

    [Test]
    public void ActiveAnimationControllersDoNotReferenceArchive()
    {
        AssertNoLiteralArchiveReference("Assets/Art/Animation/Controllers/WarriorVisual.controller");
        AssertNoLiteralArchiveReference("Assets/Art/Animation/Controllers/BasicEnemyVisual.controller");
        AssertNoLiteralArchiveReference("Assets/Art/Animation/Controllers/BossVisual.controller");
    }

    [Test]
    public void ArchiveValidationPasses()
    {
        ArchiveValidationReport report = AssetArchiveValidator.CreateReport();
        Assert.IsEmpty(report.failures, string.Join("\n", report.failures));
        Assert.Greater(report.fileCount, 0);
    }

    [Test]
    public void RuntimeVisualAssemblyDoesNotDependOnArchiveEditorTooling()
    {
        Assert.IsNull(typeof(VisualAttachmentRoot).Assembly.GetReferencedAssemblies()
            .FirstOrDefaultName("UnityEditor"));
    }

    private static void AssertNoLiteralArchiveReference(string path)
    {
        Assert.IsTrue(File.Exists(path), path);
        string text = File.ReadAllText(path);
        Assert.IsFalse(text.Contains(AssetArchiveValidator.ArchiveRoot), path + " references the asset archive path.");
        Assert.IsFalse(text.Contains("/_Archive/"), path + " references the asset archive folder.");
    }
}
#endif
