#if UNITY_EDITOR
using Game.Combat;
using Game.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class Phase8PrefabAndBossTests
{
    [Test]
    public void AllClassPrefabsHaveCoreAndAnimatorComponents()
    {
        AssertPlayerPrefab("Assets/Prefabs/Player/FighterPlayer.prefab");
        AssertPlayerPrefab("Assets/Prefabs/Player/MagePlayer.prefab");
        AssertPlayerPrefab("Assets/Prefabs/Player/ArcherPlayer.prefab");
        AssertPlayerPrefab("Assets/Prefabs/Player/HealerPlayer.prefab");
    }

    [Test]
    public void PlaceholderAnimatorControllersExist()
    {
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Player/PlayerPlaceholder.controller"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Enemies/EnemyPlaceholder.controller"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Boss/BossPlaceholder.controller"));
    }

    [Test]
    public void BossTelegraphDataSupportsPhaseEightShapes()
    {
        var slam = ScriptableObject.CreateInstance<BossTelegraphData>();
        slam.attackType = BossAttackType.Slam;
        slam.shape = TelegraphShape.Circle;

        var line = ScriptableObject.CreateInstance<BossTelegraphData>();
        line.attackType = BossAttackType.Line;
        line.shape = TelegraphShape.Line;
        line.length = 10f;

        var pulse = ScriptableObject.CreateInstance<BossTelegraphData>();
        pulse.attackType = BossAttackType.RadialPulse;
        pulse.shape = TelegraphShape.Cone;

        Assert.AreEqual(TelegraphShape.Circle, slam.shape);
        Assert.AreEqual(TelegraphShape.Line, line.shape);
        Assert.AreEqual(BossAttackType.RadialPulse, pulse.attackType);
    }

    private static void AssertPlayerPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Assert.IsNotNull(prefab);
        Assert.IsNotNull(prefab.GetComponent<CharacterController>());
        Assert.IsNotNull(prefab.GetComponent<PlayerController>());
        Assert.IsNotNull(prefab.GetComponent<ComboSystem>());
        Assert.IsNotNull(prefab.GetComponent<AbilityController>());
        Assert.IsNotNull(prefab.GetComponent<DodgeController>());
        Assert.IsNotNull(prefab.GetComponent<ParryController>());
        Assert.IsNotNull(prefab.GetComponent<Animator>());
        Assert.IsNotNull(prefab.GetComponent<Animator>().runtimeAnimatorController);
    }
}
#endif
