using UnityEngine;
using Game.Rhythm;

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

        private InputBuffer buffer;
        private int stepIndex;
        private float comboTimer;

        private void Awake()
        {
            buffer = GetComponent<InputBuffer>();
        }

        private void OnEnable()
        {
            buffer.OnResolved += HandleResolved;
        }

        private void OnDisable()
        {
            buffer.OnResolved -= HandleResolved;
        }

        private void Update()
        {
            if (profile == null || profile.steps == null || profile.steps.Length == 0) return;

            // Manual input example: left mouse fires buffer
            if (Input.GetButtonDown("Fire1"))
                buffer.RegisterPress();

            // combo timeout
            if (stepIndex > 0)
            {
                comboTimer += Time.deltaTime;
                if (comboTimer > profile.comboTimeout)
                {
                    stepIndex = 0;
                    comboTimer = 0f;
                }
            }
        }

        private void HandleResolved(RhythmGrade grade)
        {
            if (profile == null || profile.steps == null || profile.steps.Length == 0) return;

            // Clamp step
            if (stepIndex >= profile.steps.Length) stepIndex = 0;

            var step = profile.steps[stepIndex];
            float mult = judgement.DamageMultiplier(grade);
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(step.baseDamage * mult));

            DoMeleeHit(finalDamage);

            // cooldown refund concept left as a hook for ability systems
            float refund = judgement.CooldownRefund(grade);
            float effectiveCooldown = Mathf.Max(0f, step.cooldown * (1f - refund));
            // Could lock input for effectiveCooldown, or feed into ability manager

            // advance combo
            stepIndex = (stepIndex + 1) % profile.steps.Length;
            comboTimer = 0f;
        }

        private void DoMeleeHit(int damage)
        {
            Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
            Collider[] hits = Physics.OverlapSphere(center, attackRange, enemyMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                var enemy = hits[i].GetComponent<Game.Core.BaseEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
