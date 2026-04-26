using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioCuePlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;
        [SerializeField] private int sampleRate = 22050;

        private readonly Dictionary<AudioCueDefinition, AudioClip> clipCache = new Dictionary<AudioCueDefinition, AudioClip>();

        private void Awake()
        {
            audioSource = audioSource != null ? audioSource : GetComponent<AudioSource>();
            if (audioSource != null)
                audioSource.playOnAwake = false;
        }

        public void Play(AudioCueDefinition cue)
        {
            if (cue == null || audioSource == null || masterVolume <= 0f)
                return;

            AudioClip clip = GetOrCreateClip(cue);
            if (clip == null) return;

            audioSource.pitch = cue.pitch;
            audioSource.PlayOneShot(clip, Mathf.Clamp01(cue.volume * masterVolume));
        }

        public void ClearCache()
        {
            clipCache.Clear();
        }

        private AudioClip GetOrCreateClip(AudioCueDefinition cue)
        {
            if (clipCache.TryGetValue(cue, out AudioClip cached) && cached != null)
                return cached;

            int samples = Mathf.Max(1, Mathf.CeilToInt(sampleRate * Mathf.Max(0.01f, cue.duration)));
            float[] data = new float[samples];
            float frequency = Mathf.Max(20f, cue.frequency);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (i / (float)samples);
                data[i] = Sample(cue.waveform, t, frequency) * envelope;
            }

            AudioClip clip = AudioClip.Create(cue.cueId, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            clipCache[cue] = clip;
            return clip;
        }

        private static float Sample(AudioCueWaveform waveform, float time, float frequency)
        {
            switch (waveform)
            {
                case AudioCueWaveform.Square:
                    return Mathf.Sin(Mathf.PI * 2f * frequency * time) >= 0f ? 1f : -1f;
                case AudioCueWaveform.Noise:
                    return Random.value * 2f - 1f;
                default:
                    return Mathf.Sin(Mathf.PI * 2f * frequency * time);
            }
        }
    }
}
