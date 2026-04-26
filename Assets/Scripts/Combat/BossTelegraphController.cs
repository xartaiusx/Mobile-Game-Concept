using Game.Core;
using Game.Rhythm;
using Game.Audio;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class BossTelegraphController : MonoBehaviour
    {
        [SerializeField] private BossTelegraphData defaultTelegraph;
        [SerializeField] private Transform target;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioCuePlayer audioCuePlayer;
        [SerializeField] private AudioCueDefinition warningAudioCue;
        [SerializeField] private AudioCueDefinition impactAudioCue;
        [SerializeField] private float telegraphRepeatCooldown = 4f;
        [SerializeField] private float warningPulseScale = 1.15f;
        [SerializeField] private float impactDuration = 0.45f;
        [SerializeField] private int maxTargets = 16;

        private Collider[] hitCache;
        private BeatClock subscribedClock;
        private BossTelegraphData activeTelegraph;
        private int remainingBeats;
        private Vector3 impactPoint;
        private GameObject warningVfxInstance;
        private float lastTelegraphTime = float.NegativeInfinity;

        public event Action<BossTelegraphData, int, Vector3> TelegraphStarted;
        public event Action<BossTelegraphData, int> TelegraphBeat;
        public event Action<BossTelegraphData, Vector3> TelegraphImpacted;

        public bool IsTelegraphing => activeTelegraph != null;
        public int RemainingBeats => remainingBeats;
        public BossTelegraphData ActiveTelegraph => activeTelegraph;
        public Vector3 ImpactPoint => impactPoint;

        public void ApplyPhaseTuning(float cooldownMultiplier, float pulseScale)
        {
            telegraphRepeatCooldown *= Mathf.Max(0.1f, cooldownMultiplier);
            warningPulseScale = Mathf.Max(0.5f, pulseScale);
        }

        private void Awake()
        {
            hitCache = new Collider[Mathf.Max(1, maxTargets)];
            audioSource = audioSource != null ? audioSource : GetComponent<AudioSource>();
            audioCuePlayer = audioCuePlayer != null ? audioCuePlayer : GetComponent<AudioCuePlayer>();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (subscribedClock != null)
            {
                subscribedClock.OnBeat -= HandleBeat;
                subscribedClock = null;
            }
        }

        public bool BeginTelegraph()
        {
            return BeginTelegraph(defaultTelegraph);
        }

        public bool BeginTelegraph(BossTelegraphData telegraph)
        {
            if (telegraph == null || IsTelegraphing) return false;
            if (Time.time < lastTelegraphTime + telegraphRepeatCooldown) return false;

            activeTelegraph = telegraph;
            lastTelegraphTime = Time.time;
            remainingBeats = Mathf.Max(1, telegraph.beatsBeforeImpact);
            Transform resolvedTarget = target != null ? target : PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerTransform() : null;
            impactPoint = resolvedTarget != null ? resolvedTarget.position : transform.position + transform.forward * telegraph.range;

            if (telegraph.warningVfxPrefab != null)
                warningVfxInstance = Instantiate(telegraph.warningVfxPrefab, impactPoint, Quaternion.identity);

            if (audioSource != null && telegraph.audioCue != null)
                audioSource.PlayOneShot(telegraph.audioCue);
            audioCuePlayer?.Play(warningAudioCue);

            TelegraphStarted?.Invoke(activeTelegraph, remainingBeats, impactPoint);
            return true;
        }

        public void TickBeatForTests()
        {
            HandleBeat(0, 0);
        }

        private void TrySubscribe()
        {
            if (subscribedClock != null || BeatClock.Instance == null) return;
            subscribedClock = BeatClock.Instance;
            subscribedClock.OnBeat += HandleBeat;
        }

        private void HandleBeat(int beatIndex, double beatTime)
        {
            if (!IsTelegraphing) return;

            remainingBeats--;
            PulseWarning();
            audioCuePlayer?.Play(warningAudioCue);
            TelegraphBeat?.Invoke(activeTelegraph, remainingBeats);
            if (remainingBeats <= 0)
                ExecuteImpact();
        }

        private void ExecuteImpact()
        {
            BossTelegraphData telegraph = activeTelegraph;
            activeTelegraph = null;

            if (warningVfxInstance != null)
                Destroy(warningVfxInstance);

            if (telegraph.impactVfxPrefab != null)
            {
                GameObject impact = Instantiate(telegraph.impactVfxPrefab, impactPoint, Quaternion.identity);
                if (impactDuration > 0f)
                    Destroy(impact, impactDuration);
            }

            audioCuePlayer?.Play(impactAudioCue);

            TelegraphImpacted?.Invoke(telegraph, impactPoint);
            int hitCount = QueryTargets(telegraph);
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = hitCache[i];
                if (hit == null) continue;

                var character = hit.GetComponent<BaseCharacter>();
                if (character != null)
                {
                    var context = new DamageContext(gameObject, telegraph.damage, DamageType.Rhythm, RhythmGrade.Miss, true);
                    character.TakeDamage(context);
                }
            }
        }

        private void PulseWarning()
        {
            if (warningVfxInstance == null) return;
            float radius = activeTelegraph != null ? Mathf.Max(0.75f, activeTelegraph.radius) : 1f;
            warningVfxInstance.transform.position = impactPoint;
            warningVfxInstance.transform.localScale = new Vector3(radius * warningPulseScale, 0.04f, radius * warningPulseScale);
        }

        private int QueryTargets(BossTelegraphData telegraph)
        {
            switch (telegraph.attackType)
            {
                case BossTelegraphAttackType.ForwardCone:
                    return Physics.OverlapSphereNonAlloc(transform.position + transform.forward * (telegraph.range * 0.5f), Mathf.Max(telegraph.radius, telegraph.range * 0.5f), hitCache, targetMask, QueryTriggerInteraction.Ignore);
                case BossTelegraphAttackType.TargetedCircle:
                    return Physics.OverlapSphereNonAlloc(impactPoint, telegraph.radius, hitCache, targetMask, QueryTriggerInteraction.Ignore);
                default:
                    return Physics.OverlapSphereNonAlloc(transform.position, telegraph.radius, hitCache, targetMask, QueryTriggerInteraction.Ignore);
            }
        }
    }
}
