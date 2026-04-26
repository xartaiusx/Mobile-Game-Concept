using UnityEngine;
using Game.Core;

namespace Game.Classes
{
    public class Archer : BaseCharacter
    {
        [SerializeField] private int healthMultiplier = 9;
        [SerializeField] private float healthRestorationFactor = 0.3f;

        public override void InitializeStats()
        {
            CharacterName = "Archer";
            Strength = 7;
            Stamina = 6;
            Intelligence = 5;

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
            Strength += 2;
            Stamina += 2;
            Intelligence += 2;
        }

        private void UpdateHealth()
        {
            MaxHealth = Stamina * healthMultiplier;
            RestoreMissingHealthPercent(healthRestorationFactor);
        }

        private void UnlockAbilities()
        {
            Debug.Log("Archer has unlocked a new ability!");
        }

        private void ApplyAdditionalLevelUpEffects()
        {
            Debug.Log("Archer receives additional level-up bonuses.");
        }
    }
}
