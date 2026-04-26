using UnityEngine;

namespace Game.Audio
{
    public enum AudioCueWaveform
    {
        Sine,
        Square,
        Noise
    }

    [CreateAssetMenu(menuName = "Game/Audio/AudioCueDefinition")]
    public class AudioCueDefinition : ScriptableObject
    {
        public string cueId = "cue";
        [Min(20f)] public float frequency = 440f;
        [Min(0.01f)] public float duration = 0.08f;
        [Range(0f, 1f)] public float volume = 0.25f;
        [Range(0.2f, 3f)] public float pitch = 1f;
        public AudioCueWaveform waveform = AudioCueWaveform.Sine;
    }
}
