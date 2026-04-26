#if UNITY_EDITOR
using System.Collections;
using System.IO;
using Game.Combat;
using Game.Core;
using Game.Feedback;
using Game.Audio;
using Game.Animation;
using Game.Rhythm;
using Game.UI;
using Game.Classes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class VerticalSlicePlayModeTests
{
    [UnityTest]
    public IEnumerator VerticalSliceSceneStartsWithCoreGameplayObjects()
    {
        yield return LoadVerticalSlice();

        Assert.IsNotNull(Object.FindAnyObjectByType<PlayerManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<GameManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatClock>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmJudgement>());

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Fighter>());
        Assert.AreEqual("Warrior", player.GetComponent<BaseCharacter>().CharacterName);
        Assert.IsNotNull(player.GetComponent<CharacterController>());
        Assert.IsNotNull(player.GetComponent<PlayerController>());
        Assert.IsNotNull(player.GetComponent<ComboSystem>());
        Assert.AreEqual(AttackTimingState.Ready, player.GetComponent<ComboSystem>().TimingState);
        Assert.IsNotNull(player.GetComponent<AbilityController>());
        Assert.IsNotNull(player.GetComponent<DodgeController>());
        Assert.IsNotNull(player.GetComponent<ParryController>());
        Assert.IsNotNull(player.GetComponent<CombatAnimationBridge>());
        Assert.IsNotNull(player.GetComponent<AnimationEventRelay>());
        Assert.IsNotNull(player.GetComponent<InventorySystem>());
        Assert.IsNotNull(player.GetComponent<HitReactionController>());
        Assert.IsNotNull(player.GetComponent<Animator>());
        Assert.IsNotNull(player.GetComponent<Animator>().runtimeAnimatorController);

        Assert.IsNotNull(Object.FindAnyObjectByType<MeleeEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RangedEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BossEnemy>(FindObjectsInactive.Include));
        Assert.IsNotNull(Object.FindAnyObjectByType<BossTelegraphController>(FindObjectsInactive.Include));
        Assert.IsNotNull(Object.FindAnyObjectByType<ArenaController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ScoreSystem>());
        Assert.IsNotNull(Object.FindAnyObjectByType<PickupSpawner>());
        Assert.IsNull(Object.FindAnyObjectByType<ClassSwapDebugController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<Canvas>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatBarUI>());
        Assert.IsNotNull(Object.FindAnyObjectByType<VerticalSliceHud>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmFeedbackController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<AudioCuePlayer>());
    }

    [UnityTest]
    public IEnumerator VerticalSliceHasNoDuplicateCriticalSingletons()
    {
        yield return LoadVerticalSlice();

        Assert.AreEqual(1, Object.FindObjectsByType<PlayerManager>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<GameManager>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<BeatClock>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<RhythmJudgement>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<ScoreSystem>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<VerticalSliceHud>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
    }

    [UnityTest]
    public IEnumerator VerticalSliceUsesWarriorOnlyAndLegacyClassSwapIsInactive()
    {
        yield return LoadVerticalSlice();

        ClassSwapDebugController classSwap = Object.FindAnyObjectByType<ClassSwapDebugController>(FindObjectsInactive.Include);
        Assert.IsTrue(classSwap == null || !classSwap.gameObject.activeInHierarchy);
        SimpleFollowCamera followCamera = Object.FindAnyObjectByType<SimpleFollowCamera>();
        VerticalSliceHud hud = Object.FindAnyObjectByType<VerticalSliceHud>();
        ArenaController arena = Object.FindAnyObjectByType<ArenaController>();
        Assert.IsNotNull(followCamera);
        Assert.IsNotNull(hud);
        Assert.IsNotNull(arena);

        Directory.CreateDirectory("Artifacts/WarriorEndless");
        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Fighter>());
        Assert.IsNull(player.GetComponent<Mage>());
        Assert.IsNull(player.GetComponent<Archer>());
        Assert.IsNull(player.GetComponent<Healer>());
        Assert.AreEqual(player.transform, PlayerManager.Instance.GetPlayerTransform());
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
        Assert.AreEqual(player.transform, SerializedTransform(followCamera, "target"));
        Assert.AreEqual(player.GetComponent<ComboSystem>(), SerializedObjectReference<ComboSystem>(hud, "comboSystem"));
        Assert.AreEqual(player.GetComponent<BaseCharacter>(), SerializedObjectReference<BaseCharacter>(arena, "player"));

        player.GetComponent<PlayerSimulationController>().SetSimulationEnabled(true);
        ScreenCapture.CaptureScreenshot("Artifacts/WarriorEndless/warrior_playmode_test.png");
    }

    [UnityTest]
    public IEnumerator VerticalSliceEndlessArenaCanClearWaveAndIncrement()
    {
        yield return LoadVerticalSlice();

        ArenaController arena = Object.FindAnyObjectByType<ArenaController>();
        ScoreSystem score = Object.FindAnyObjectByType<ScoreSystem>();
        Assert.IsNotNull(arena);
        Assert.IsNotNull(score);

        int startingWave = arena.CurrentWave;
        foreach (BaseEnemy enemy in Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude))
        {
            if (enemy != null)
                enemy.TakeDamage(9999);
        }

        Assert.AreEqual(ArenaState.WaveCleared, arena.State);
        Assert.AreNotEqual(ArenaState.Victory, arena.State);
        Assert.Greater(score.Score, 0);

        arena.BeginNextWaveForTests();

        Assert.AreEqual(startingWave + 1, arena.CurrentWave);
        Assert.AreNotEqual(ArenaState.Victory, arena.State);
    }

    [UnityTest]
    public IEnumerator VerticalSliceSmokeTestHasNoMissingScripts()
    {
        yield return LoadVerticalSlice();

        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
        Assert.AreEqual(1, Object.FindObjectsByType<BeatClock>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<ScoreSystem>(FindObjectsInactive.Exclude).Length);
        Assert.IsNotNull(Object.FindAnyObjectByType<VerticalSliceHud>());
        Assert.Greater(Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude).Length, 0);

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go), go.name + " has missing scripts.");
    }

    private static IEnumerator LoadVerticalSlice()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
        Assert.IsNotNull(load);
        while (!load.isDone)
            yield return null;

        yield return null;
    }

    private static Transform SerializedTransform(Object target, string propertyName)
    {
        return new SerializedObject(target).FindProperty(propertyName).objectReferenceValue as Transform;
    }

    private static T SerializedObjectReference<T>(Object target, string propertyName) where T : Object
    {
        return new SerializedObject(target).FindProperty(propertyName).objectReferenceValue as T;
    }
}
#endif
