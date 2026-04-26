#if UNITY_EDITOR
using Game.Combat;
using Game.Core;
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
        Assert.IsNotNull(player.GetComponent<AbilityController>());
        Assert.IsNotNull(player.GetComponent<DodgeController>());
        Assert.IsNotNull(player.GetComponent<ParryController>());

        Assert.IsNotNull(Object.FindAnyObjectByType<MeleeEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RangedEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BossEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BossTelegraphController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<Canvas>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatBarUI>());
        Assert.IsNotNull(Object.FindAnyObjectByType<VerticalSliceHud>());
    }
}
#endif
