using Game.Core;
using Game.Rhythm;
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
        [SerializeField] private int maxTargets = 16;

        private Collider[] hitCache;
        private BeatClock subscribedClock;
        private BossTelegraphData activeTelegraph;
        private int remainingBeats;
        private Vector3 impactPoint;
        private GameObject warningVfxInstance;

        public event Action<BossTelegraphData, int, Vector3> TelegraphStarted;
        public event Action<BossTelegraphData, int> TelegraphBeat;
        public event Action<BossTelegraphData, Vector3> TelegraphImpacted;

        public bool IsTelegraphing => activeTelegraph != null;
        public int RemainingBeats => remainingBeats;
        public BossTelegraphData ActiveTelegraph => activeTelegraph;
        public Vector3 ImpactPoint => impactPoint;

        private void Awake()
        {
            hitCache = new Collider[Mathf.Max(1, maxTargets)];
            audioSource = audioSource != null ? audioSource : GetComponent<AudioSource>();
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

            activeTelegraph = telegraph;
            remainingBeats = Mathf.Max(1, telegraph.beatsBeforeImpact);
            Transform resolvedTarget = target != null ? target : PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerTransform() : null;
            impactPoint = resolvedTarget != null ? resolvedTarget.position : transform.position + transform.forward * telegraph.range;

            if (telegraph.warningVfxPrefab != null)
                warningVfxInstance = Instantiate(telegraph.warningVfxPrefab, impactPoint, Quaternion.identity);

            if (audioSource != null && telegraph.audioCue != null)
                audioSource.PlayOneShot(telegraph.audioCue);

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
                Instantiate(telegraph.impactVfxPrefab, impactPoint, Quaternion.identity);

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
