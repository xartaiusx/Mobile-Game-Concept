#if UNITY_EDITOR
using Game.Combat;
using Game.Core;
using Game.AI.Enemies;
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
    public void CoreVisiblePrefabsHaveRenderableMaterials()
    {
        AssertRenderableMaterials("Assets/Prefabs/Player/FighterPlayer.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Player/MagePlayer.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Player/ArcherPlayer.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Player/HealerPlayer.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Enemies/MeleeEnemy.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Enemies/RangedEnemy.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Enemies/BossEnemy.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Projectiles/BasicProjectile.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Pickups/GoldPickup.prefab");
        AssertRenderableMaterials("Assets/Prefabs/Pickups/HealthPotionPickup.prefab");
    }

    [Test]
    public void ProjectileAbilityAssetsReferenceProjectilePrefab()
    {
        AssertProjectileAbilityWiring("Assets/Prefabs/Player/MagePlayer.prefab", "Assets/ScriptableObjects/Abilities/MageBolt.asset");
        AssertProjectileAbilityWiring("Assets/Prefabs/Player/ArcherPlayer.prefab", "Assets/ScriptableObjects/Abilities/ArcherShot.asset");
    }

    [Test]
    public void BossPhaseAssetsReferenceTelegraphData()
    {
        AssertBossPhaseTelegraphs("Assets/ScriptableObjects/Boss/BossPhaseOne.asset");
        AssertBossPhaseTelegraphs("Assets/ScriptableObjects/Boss/BossPhaseTwo.asset");
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

    private static void AssertRenderableMaterials(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Assert.IsNotNull(prefab, path);
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        Assert.IsNotEmpty(renderers, path);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            Assert.IsNotEmpty(materials, path + " renderer has no material slots.");
            for (int j = 0; j < materials.Length; j++)
                Assert.IsNotNull(materials[j], path + " has a missing material.");
        }
    }

    private static void AssertProjectileAbilityWiring(string prefabPath, string abilityPath)
    {
        AbilityDefinition ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(abilityPath);
        Assert.IsNotNull(ability, abilityPath);
        Assert.AreEqual(AbilityExecutionStyle.ProjectileLike, ability.executionStyle, abilityPath);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.IsNotNull(prefab, prefabPath);
        AbilityController controller = prefab.GetComponent<AbilityController>();
        Assert.IsNotNull(controller, prefabPath);

        var serialized = new SerializedObject(controller);
        GameObject projectilePrefab = serialized.FindProperty("projectilePrefab").objectReferenceValue as GameObject;
        ObjectPool projectilePool = serialized.FindProperty("projectilePool").objectReferenceValue as ObjectPool;
        Assert.IsNotNull(projectilePrefab, prefabPath + " missing projectile prefab.");
        Assert.IsNotNull(projectilePool, prefabPath + " missing projectile pool.");
        Assert.IsNotNull(projectilePrefab.GetComponent<PoolableProjectile>(), prefabPath + " projectile is not pool-ready.");
    }

    private static void AssertBossPhaseTelegraphs(string path)
    {
        BossPhaseData phase = AssetDatabase.LoadAssetAtPath<BossPhaseData>(path);
        Assert.IsNotNull(phase, path);
        Assert.IsNotNull(phase.telegraphs, path + " telegraph array is null.");
        Assert.IsNotEmpty(phase.telegraphs, path + " has no telegraph data.");
        for (int i = 0; i < phase.telegraphs.Length; i++)
            Assert.IsNotNull(phase.telegraphs[i], path + " has a missing telegraph entry.");
    }
}
#endif
