using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// BaseCharacter defines shared health, stats, damage, healing, and leveling behavior.
    /// </summary>
    public abstract class BaseCharacter : MonoBehaviour
    {
        [SerializeField] private string characterName = "Character";
        [SerializeField] private int level = 1;
        [SerializeField] private int strength;
        [SerializeField] private int stamina;
        [SerializeField] private int intelligence;
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private bool initializeOnAwake = true;

        private readonly List<IDamageResponder> damageResponders = new List<IDamageResponder>(4);

        public string CharacterName { get => characterName; protected set => characterName = value; }
        public int Level => level;
        public int Strength { get => strength; protected set => strength = Mathf.Max(0, value); }
        public int Stamina { get => stamina; protected set => stamina = Mathf.Max(0, value); }
        public int Intelligence { get => intelligence; protected set => intelligence = Mathf.Max(0, value); }
        public int MaxHealth { get => maxHealth; protected set => maxHealth = Mathf.Max(1, value); }
        public int CurrentHealth { get => currentHealth; protected set => currentHealth = Mathf.Clamp(value, 0, MaxHealth); }
        public bool IsAlive => CurrentHealth > 0;

        public event Action<BaseCharacter> OnDamaged;
        public event Action<BaseCharacter> OnHealed;
        public event Action<BaseCharacter> OnDeath;
        public event Action<BaseCharacter> OnLevelChanged;

        protected virtual void Awake()
        {
            RefreshDamageResponders();
            if (initializeOnAwake)
                InitializeStats();
        }

        public void RefreshDamageResponders()
        {
            damageResponders.Clear();
            GetComponents(damageResponders);
        }

        public abstract void InitializeStats();

        public virtual void LevelUp()
        {
            level++;
            OnLevelChanged?.Invoke(this);
        }

        protected void SetLevel(int value)
        {
            level = Mathf.Max(1, value);
            OnLevelChanged?.Invoke(this);
        }

        protected void RestoreMissingHealthPercent(float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            int missing = MaxHealth - CurrentHealth;
            Heal(Mathf.CeilToInt(missing * fraction));
        }

        public void TakeDamage(int damage)
        {
            TakeDamage(new DamageContext(null, damage));
        }

        public void TakeDamage(DamageContext context)
        {
            if (!IsAlive || context.amount <= 0) return;

            RefreshDamageResponders();
            for (int i = 0; i < damageResponders.Count; i++)
            {
                if (damageResponders[i] != null && damageResponders[i].TryModifyDamage(ref context))
                    break;
            }

            if (context.amount <= 0) return;

            CurrentHealth -= context.amount;
            OnDamaged?.Invoke(this);

            if (CurrentHealth == 0)
                Die();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth += amount;
            OnHealed?.Invoke(this);
        }

        protected virtual void Die()
        {
            Debug.Log(CharacterName + " has died.");
            OnDeath?.Invoke(this);
        }
    }
}
