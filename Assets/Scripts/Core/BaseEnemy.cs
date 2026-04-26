using UnityEngine;
using System;

namespace Game.Core
{
    /// <summary>
    /// Base class for AI enemies. Owns health, attack cadence, and defeat events.
    /// </summary>
    public abstract class BaseEnemy : MonoBehaviour
    {
        [SerializeField] private string enemyName = "Enemy";
        [SerializeField] private int maxHealth = 25;
        [SerializeField] private int health = 25;
        [SerializeField] private int attackDamage = 5;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float invulnerabilityDuration = 0.15f;

        protected float attackTimer;
        protected CharacterController controller;
        private bool isInvulnerable;
        private float invulnerabilityTimer;
        private bool defeated;

        public static event Action<BaseEnemy> EnemyDefeatedGlobal;
        public static event Action<BaseEnemy, DamageContext> EnemyDamagedGlobal;
        public event Action<BaseEnemy> AttackStarted;
        public event Action<BaseEnemy, DamageContext> Damaged;
        public event Action<BaseEnemy> Defeated;

        public string EnemyName => enemyName;
        public int MaxHealth => maxHealth;
        public int Health
        {
            get => health;
            protected set => health = Mathf.Clamp(value, 0, maxHealth);
        }
        public int AttackDamage { get => attackDamage; protected set => attackDamage = Mathf.Max(0, value); }
        public float AttackInterval { get => attackInterval; protected set => attackInterval = Mathf.Max(0.05f, value); }
        public float MoveSpeed { get => moveSpeed; protected set => moveSpeed = Mathf.Max(0f, value); }
        public float HealthFraction => maxHealth <= 0 ? 0f : (float)Health / maxHealth;
        public bool IsAlive => !defeated && Health > 0;

        protected virtual void Awake()
        {
            Health = health <= 0 ? maxHealth : health;
        }

        protected virtual void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        protected virtual void Update()
        {
            HandleAttackTimer();
            HandleMovement();
            HandleInvulnerability();
        }

        private void HandleAttackTimer()
        {
            if (attackInterval <= 0f) return;
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                PerformAttack();
                attackTimer = 0f;
            }
        }

        private void HandleInvulnerability()
        {
            if (!isInvulnerable) return;

            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }

        protected virtual void HandleMovement()
        {
        }

        public abstract void PerformAttack();

        protected void NotifyAttackStarted()
        {
            AttackStarted?.Invoke(this);
        }

        public void TakeDamage(int damage)
        {
            TakeDamage(new DamageContext(null, damage));
        }

        public void TakeDamage(DamageContext context)
        {
            if (!IsAlive || context.amount <= 0) return;
            if (isInvulnerable) return;

            Health -= context.amount;
            isInvulnerable = invulnerabilityDuration > 0f;
            invulnerabilityTimer = invulnerabilityDuration;
            Damaged?.Invoke(this, context);
            EnemyDamagedGlobal?.Invoke(this, context);

            if (Health <= 0)
                Die();
        }

        protected virtual void Die()
        {
            if (defeated) return;
            defeated = true;
            Debug.Log(EnemyName + " has been defeated.");
            Defeated?.Invoke(this);
            EnemyDefeatedGlobal?.Invoke(this);
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }

        public void ApplyPhaseStats(float moveSpeedMultiplier, float attackIntervalMultiplier)
        {
            MoveSpeed *= Mathf.Max(0.01f, moveSpeedMultiplier);
            AttackInterval *= Mathf.Max(0.01f, attackIntervalMultiplier);
        }

        public void ApplyEndlessDifficulty(float healthMultiplier, float damageMultiplier, float moveSpeedMultiplier, float attackIntervalMultiplier)
        {
            int scaledMaxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * Mathf.Max(0.01f, healthMultiplier)));
            maxHealth = scaledMaxHealth;
            Health = scaledMaxHealth;
            AttackDamage = Mathf.RoundToInt(AttackDamage * Mathf.Max(0.01f, damageMultiplier));
            MoveSpeed *= Mathf.Max(0.01f, moveSpeedMultiplier);
            AttackInterval *= Mathf.Max(0.01f, attackIntervalMultiplier);
        }

        public virtual void Stagger(float duration)
        {
            attackTimer = 0f;
        }
    }
}
