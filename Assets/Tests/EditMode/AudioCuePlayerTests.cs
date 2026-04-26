#if UNITY_EDITOR
using Game.Audio;
using NUnit.Framework;
using UnityEngine;

public class AudioCuePlayerTests
{
    [Test]
    public void PlayWithNullCueDoesNotThrow()
    {
        var go = new GameObject("audio-cue-player");
        var player = go.AddComponent<AudioCuePlayer>();

        Assert.DoesNotThrow(() => player.Play(null));
    }

    [Test]
    public void ProceduralCueCanBePlayedSafely()
    {
        var go = new GameObject("audio-cue-player-with-source");
        go.AddComponent<AudioSource>();
        var player = go.AddComponent<AudioCuePlayer>();
        var cue = ScriptableObject.CreateInstance<AudioCueDefinition>();
        cue.cueId = "test_cue";
        cue.frequency = 440f;
        cue.duration = 0.02f;
        cue.volume = 0.1f;

        Assert.DoesNotThrow(() => player.Play(cue));
    }
}
#endif
