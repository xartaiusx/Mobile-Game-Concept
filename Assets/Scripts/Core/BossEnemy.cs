using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Game.AI.Enemies;
using Game.Combat;

namespace Game.Core
{
    public enum BossAttackMode { Melee, Ranged }

    /// <summary>
    /// Represents a boss enemy with phase-swappable melee/ranged behavior.
    /// </summary>
    public class BossEnemy : BaseEnemy
    {
        [SerializeField] private float attackRange = 12f;
        [SerializeField] private float chaseRange = 20f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private ObjectPool projectilePool;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float specialAttackCooldown = 10f;
        [SerializeField] private BossAttackMode attackMode = BossAttackMode.Melee;
        [SerializeField] private int burstCount = 1;
        [SerializeField] private float burstInterval = 0.2f;
        [SerializeField] private BossTelegraphController telegraphController;
        [SerializeField] private BossTelegraphData specialTelegraph;

        private NavMeshAgent navMeshAgent;
        private float lastSpecialAttackTime = float.NegativeInfinity;

        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        private Transform PlayerTarget => PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerTransform() : null;

        protected override void Start()
        {
            base.Start();
            navMeshAgent = GetComponent<NavMeshAgent>();
            telegraphController = telegraphController != null ? telegraphController : GetComponent<BossTelegraphController>();
            if (navMeshAgent != null)
                navMeshAgent.speed = MoveSpeed;

        }

        protected override void HandleMovement()
        {
            if (PlayerTarget == null || navMeshAgent == null)
                return;

            float distance = Vector3.Distance(transform.position, PlayerTarget.position);
            if (distance <= chaseRange && distance > attackRange)
                navMeshAgent.SetDestination(PlayerTarget.position);
            else if (distance <= attackRange)
                navMeshAgent.ResetPath();
        }

        public override void PerformAttack()
        {
            if (PlayerTarget == null)
                return;

            float distance = Vector3.Distance(transform.position, PlayerTarget.position);
            if (distance > attackRange) return;

            NotifyAttackStarted();
            if (attackMode == BossAttackMode.Ranged)
                StartCoroutine(FireBurst(PlayerTarget));
            else
                PerformMeleeHit(PlayerTarget);

            if (Time.time >= lastSpecialAttackTime + specialAttackCooldown)
            {
                lastSpecialAttackTime = Time.time;
                StartCoroutine(SpecialAttack());
            }
        }

        public void ApplyPhase(BossPhaseData phase)
        {
            if (phase == null) return;

            attackMode = phase.rangedMode ? BossAttackMode.Ranged : BossAttackMode.Melee;
            burstCount = Mathf.Max(1, phase.burstCount);
            burstInterval = Mathf.Max(0.01f, phase.burstInterval);
            ApplyPhaseStats(phase.moveSpeedMultiplier, phase.attackCooldownMultiplier);

            if (navMeshAgent != null)
                navMeshAgent.speed = MoveSpeed;

            if (phase.telegraphs != null && phase.telegraphs.Length > 0 && phase.telegraphs[0] != null)
            {
                specialTelegraph = phase.telegraphs[0];
                if (telegraphController != null)
                    telegraphController.SetDefaultTelegraph(specialTelegraph);
            }
        }

        private void PerformMeleeHit(Transform target)
        {
            var character = target.GetComponent<BaseCharacter>();
            if (character != null)
                character.TakeDamage(new DamageContext(gameObject, AttackDamage, DamageType.Physical, Game.Rhythm.RhythmGrade.Miss, true));
        }

        private IEnumerator FireBurst(Transform target)
        {
            int count = Mathf.Max(1, burstCount);
            for (int i = 0; i < count; i++)
            {
                FireProjectile(target);
                if (i < count - 1)
                    yield return new WaitForSeconds(burstInterval);
            }
        }

        private void FireProjectile(Transform target)
        {
            if (target == null || (projectilePrefab == null && projectilePool == null)) return;

            Vector3 spawnPosition = transform.position + transform.forward;
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion rotation = direction.sqrMagnitude > 0f ? Quaternion.LookRotation(direction) : transform.rotation;
            GameObject projectile = projectilePool != null
                ? projectilePool.Spawn(spawnPosition, rotation)
                : Instantiate(projectilePrefab, spawnPosition, rotation);

            if (projectile == null) return;

            var poolable = projectile.GetComponent<PoolableProjectile>();
            if (poolable != null)
            {
                poolable.Initialize(target.position, projectileSpeed, AttackDamage, projectilePool, gameObject);
                return;
            }

            var projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
                projectileScript.Initialize(target.position, projectileSpeed, AttackDamage, gameObject);
        }

        private IEnumerator SpecialAttack()
        {
            if (telegraphController != null && specialTelegraph != null)
            {
                telegraphController.BeginTelegraph(specialTelegraph);
                yield break;
            }

            Debug.Log(EnemyName + " prepares a special attack, but no telegraph data is assigned.");
            yield return null;
        }
    }
}
