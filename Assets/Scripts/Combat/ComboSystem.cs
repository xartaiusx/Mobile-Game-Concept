using UnityEngine;
using Game.Rhythm;
using Game.Systems;
using Game.Core;
using System;

namespace Game.Combat
{
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
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private bool readLegacyInputDirectly;
        [SerializeField] private int maxHitColliders = 12;

        private InputBuffer buffer;
        private int stepIndex;
        private float comboTimer;
        private float inputCooldownRemaining;
        private Collider[] hitCache;

        public event Action<int, RhythmGrade, int> ComboStepResolved;
        public event Action<int, RhythmGrade, int, BaseEnemy> DamageDealt;
        public event Action ComboReset;

        public int CurrentStepIndex => stepIndex;
        public float InputCooldownRemaining => inputCooldownRemaining;
        public int CurrentComboCount => stepIndex;

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

            // Clamp step
            if (stepIndex >= profile.steps.Length) stepIndex = 0;

            var step = profile.steps[stepIndex];
            float mult = judgement != null ? judgement.DamageMultiplier(grade) : 1f;
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(step.baseDamage * mult));

            DoMeleeHit(finalDamage, grade);
            ComboStepResolved?.Invoke(stepIndex + 1, grade, finalDamage);

            float refund = judgement != null ? judgement.CooldownRefund(grade) : 0f;
            inputCooldownRemaining = Mathf.Max(0f, step.cooldown * (1f - refund));
            Analytics.LogBeat(grade, stepIndex + 1);

            stepIndex = (stepIndex + 1) % profile.steps.Length;
            comboTimer = 0f;
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
    }
}
