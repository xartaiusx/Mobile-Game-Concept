using UnityEngine;
using Game.Rhythm;
using Game.Systems;
using Game.Core;
using System;
using System.Collections;

namespace Game.Combat
{
    public enum AttackTimingState
    {
        Ready,
        Windup,
        Active,
        Recovery
    }

    /// <summary>
    /// Applies rhythm grades to combo steps and computes final damage.
    /// Drive this via InputBuffer.OnResolved.
    /// </summary>
    [RequireComponent(typeof(InputBuffer))]
    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private ComboProfile profile;
        [SerializeField] private RhythmJudgement judgement;
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private AttackTimingData attackTiming;
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private bool readLegacyInputDirectly;
        [SerializeField] private int maxHitColliders = 12;

        private InputBuffer buffer;
        private int stepIndex;
        private float comboTimer;
        private float inputCooldownRemaining;
        private Collider[] hitCache;
        private Coroutine attackRoutine;
        private int activeStepIndex;
        private int activeDamage;
        private RhythmGrade activeGrade = RhythmGrade.Miss;
        private bool activeHitApplied;

        public event Action<int, RhythmGrade> AttackWindupStarted;
        public event Action<int, RhythmGrade, int> AttackActiveStarted;
        public event Action<int, RhythmGrade> AttackRecovered;
        public event Action<int, RhythmGrade, int> ComboStepResolved;
        public event Action<int, RhythmGrade, int, BaseEnemy> DamageDealt;
        public event Action ComboReset;

        public int CurrentStepIndex => stepIndex;
        public float InputCooldownRemaining => inputCooldownRemaining;
        public int CurrentComboCount => stepIndex;
        public AttackTimingState TimingState { get; private set; } = AttackTimingState.Ready;

        private void Awake()
        {
            buffer = GetComponent<InputBuffer>();
            judgement = judgement != null ? judgement : GetComponent<RhythmJudgement>();
            hitCache = new Collider[Mathf.Max(1, maxHitColliders)];
        }

        private void OnEnable()
        {
            if (buffer != null)
                buffer.OnResolved += HandleResolved;
        }

        private void OnDisable()
        {
            if (buffer != null)
                buffer.OnResolved -= HandleResolved;
        }

        private void Update()
        {
            if (profile == null || profile.steps == null || profile.steps.Length == 0) return;

            if (inputCooldownRemaining > 0f)
                inputCooldownRemaining -= Time.deltaTime;

            if (readLegacyInputDirectly && inputCooldownRemaining <= 0f && buffer != null && Input.GetButtonDown("Fire1"))
                buffer.RegisterPress();

            // combo timeout
            if (stepIndex > 0)
            {
                comboTimer += Time.deltaTime;
                if (comboTimer > profile.comboTimeout)
                {
                    ResetCombo();
                }
            }
        }

        private void HandleResolved(RhythmGrade grade)
        {
            if (profile == null || profile.steps == null || profile.steps.Length == 0) return;
            if (inputCooldownRemaining > 0f) return;
            if (TimingState != AttackTimingState.Ready) return;

            // Clamp step
            if (stepIndex >= profile.steps.Length) stepIndex = 0;

            var step = profile.steps[stepIndex];
            float mult = judgement != null ? judgement.DamageMultiplier(grade) : 1f;
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(step.baseDamage * mult));

            attackRoutine = StartCoroutine(ResolveAttackRoutine(stepIndex, step.cooldown, finalDamage, grade));
        }

        public void ResolveAttackForTests(RhythmGrade grade)
        {
            HandleResolved(grade);
        }

        private IEnumerator ResolveAttackRoutine(int resolvedStepIndex, float baseCooldown, int finalDamage, RhythmGrade grade)
        {
            activeStepIndex = resolvedStepIndex;
            activeDamage = finalDamage;
            activeGrade = grade;
            activeHitApplied = false;

            TimingState = AttackTimingState.Windup;
            AttackWindupStarted?.Invoke(resolvedStepIndex + 1, grade);

            float windup = attackTiming != null ? attackTiming.windupSeconds : 0f;
            if (windup > 0f)
                yield return new WaitForSeconds(windup);

            BeginAttackActiveWindow();

            float active = attackTiming != null ? attackTiming.activeSeconds : 0f;
            if (active > 0f)
                yield return new WaitForSeconds(active);

            EndAttackActiveWindow();

            float refund = judgement != null ? judgement.CooldownRefund(grade) : 0f;
            float recoveryMultiplier = grade == RhythmGrade.Perfect && attackTiming != null && attackTiming.canCancelOnPerfect ? 0.65f : 1f;
            inputCooldownRemaining = Mathf.Max(0f, baseCooldown * (1f - refund) * recoveryMultiplier);
            Analytics.LogBeat(grade, resolvedStepIndex + 1);

            stepIndex = (stepIndex + 1) % profile.steps.Length;
            comboTimer = 0f;

            float recovery = attackTiming != null ? attackTiming.recoverySeconds * recoveryMultiplier : 0f;
            if (recovery > 0f)
                yield return new WaitForSeconds(recovery);

            FinishRecovery();
            attackRoutine = null;
        }

        public void BeginAttackActiveWindow()
        {
            if (TimingState != AttackTimingState.Windup)
                return;

            TimingState = AttackTimingState.Active;
            AttackActiveStarted?.Invoke(activeStepIndex + 1, activeGrade, activeDamage);
            if (!activeHitApplied)
            {
                DoMeleeHit(activeDamage, activeGrade);
                ComboStepResolved?.Invoke(activeStepIndex + 1, activeGrade, activeDamage);
                activeHitApplied = true;
            }
        }

        public void EndAttackActiveWindow()
        {
            if (TimingState != AttackTimingState.Active) return;
            TimingState = AttackTimingState.Recovery;
        }

        public void FinishRecovery()
        {
            if (TimingState == AttackTimingState.Ready) return;
            TimingState = AttackTimingState.Ready;
            AttackRecovered?.Invoke(activeStepIndex + 1, activeGrade);
        }

        private void DoMeleeHit(int damage, RhythmGrade grade)
        {
            Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
            int count = Physics.OverlapSphereNonAlloc(center, attackRange, hitCache, enemyMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var enemy = hitCache[i].GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(new DamageContext(gameObject, damage, DamageType.Rhythm, grade, true));
                    DamageDealt?.Invoke(stepIndex + 1, grade, damage, enemy);
                }
            }
        }

        private void ResetCombo()
        {
            if (stepIndex == 0 && comboTimer <= 0f) return;
            stepIndex = 0;
            comboTimer = 0f;
            ComboReset?.Invoke();
        }

        public void CancelAttackForTests()
        {
            if (attackRoutine != null)
                StopCoroutine(attackRoutine);
            attackRoutine = null;
            TimingState = AttackTimingState.Ready;
        }
    }
}
