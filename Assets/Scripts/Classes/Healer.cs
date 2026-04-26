using System;
using UnityEngine;
using Game.Core;

namespace Game.Classes
{
    public class Healer : BaseCharacter
    {
        [SerializeField] private int healthMultiplier = 11;
        [SerializeField] private float healthRestorationFactor = 0.4f;

        public static event Action<Healer> OnLevelUpEffectsApplied;

        public override void InitializeStats()
        {
            CharacterName = "Healer";
            Strength = 4;
            Stamina = 7;
            Intelligence = 10;

            MaxHealth = Stamina * healthMultiplier;
            CurrentHealth = MaxHealth;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            ApplyStatIncreases();
            UpdateHealth();
            UnlockAbilities();
            ApplyAdditionalLevelUpEffects();
        }

        private void ApplyStatIncreases()
        {
            Strength += 1;
            Stamina += 3;
            Intelligence += 3;
        }

        private void UpdateHealth()
        {
            MaxHealth = Stamina * healthMultiplier;
            RestoreMissingHealthPercent(healthRestorationFactor);
        }

        private void UnlockAbilities()
        {
            Debug.Log("Healer has unlocked a new ability!");
        }

        private void ApplyAdditionalLevelUpEffects()
        {
            OnLevelUpEffectsApplied?.Invoke(this);
            Debug.Log("Healer receives additional level-up bonuses.");
        }
    }
}
