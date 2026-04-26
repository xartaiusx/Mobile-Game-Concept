#if UNITY_EDITOR
using Game.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class Phase6PrefabWiringTests
{
    [Test]
    public void MageAndArcherAbilitiesAreProjectileLike()
    {
        var mage = AssetDatabase.LoadAssetAtPath<AbilityDefinition>("Assets/ScriptableObjects/Abilities/MageBolt.asset");
        var archer = AssetDatabase.LoadAssetAtPath<AbilityDefinition>("Assets/ScriptableObjects/Abilities/ArcherShot.asset");

        Assert.IsNotNull(mage);
        Assert.IsNotNull(archer);
        Assert.AreEqual(AbilityExecutionStyle.ProjectileLike, mage.executionStyle);
        Assert.AreEqual(AbilityExecutionStyle.ProjectileLike, archer.executionStyle);
        Assert.Greater(mage.range, 6f);
        Assert.Greater(archer.range, mage.range);
    }

    [Test]
    public void PlayerPrefabsHaveProjectilePoolForAbilities()
    {
        AssertHasProjectilePool("Assets/Prefabs/Player/MagePlayer.prefab");
        AssertHasProjectilePool("Assets/Prefabs/Player/ArcherPlayer.prefab");
    }

    private static void AssertHasProjectilePool(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Assert.IsNotNull(prefab);

        var controller = prefab.GetComponent<AbilityController>();
        Assert.IsNotNull(controller);

        var serialized = new SerializedObject(controller);
        Assert.IsNotNull(serialized.FindProperty("projectilePrefab").objectReferenceValue);
        Assert.IsNotNull(serialized.FindProperty("projectilePool").objectReferenceValue);
    }
}
#endif
