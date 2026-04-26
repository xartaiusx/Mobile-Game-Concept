#if UNITY_EDITOR
using System.IO;
using Game.Combat;
using Game.Core;
using Game.Feedback;
using Game.Audio;
using Game.Animation;
using Game.Rhythm;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VerticalSlicePlayModeTests
{
    [Test]
    public void VerticalSliceSceneStartsWithCoreGameplayObjects()
    {
        SceneManager.LoadScene("VerticalSlice");

        Assert.IsNotNull(Object.FindAnyObjectByType<PlayerManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<GameManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatClock>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmJudgement>());

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player);
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
        Assert.IsNotNull(Object.FindAnyObjectByType<BossEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BossTelegraphController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ArenaController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ScoreSystem>());
        Assert.IsNotNull(Object.FindAnyObjectByType<PickupSpawner>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ClassSwapDebugController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<Canvas>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatBarUI>());
        Assert.IsNotNull(Object.FindAnyObjectByType<VerticalSliceHud>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmFeedbackController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<AudioCuePlayer>());
    }

    [Test]
    public void VerticalSliceClassSwapSimulationKeepsSingleManagedPlayer()
    {
        SceneManager.LoadScene("VerticalSlice");

        ClassSwapDebugController classSwap = Object.FindAnyObjectByType<ClassSwapDebugController>();
        Assert.IsNotNull(classSwap);

        Directory.CreateDirectory("Artifacts/Phase8Frames");
        string[] classNames = { "Fighter", "Mage", "Archer", "Healer" };
        for (int i = 0; i < classNames.Length; i++)
        {
            GameObject player = classSwap.SwapToClass(classNames[i]);
            Assert.IsNotNull(player);
            Assert.AreEqual(player.transform, PlayerManager.Instance.GetPlayerTransform());
            Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
            Assert.IsNotNull(player.GetComponent<Animator>());
            Assert.IsNotNull(player.GetComponent<PlayerSimulationController>());

            player.GetComponent<PlayerSimulationController>().SetSimulationEnabled(true);
            ScreenCapture.CaptureScreenshot($"Artifacts/Phase8Frames/{classNames[i]}_playmode_test.png");
        }
    }
}
#endif
