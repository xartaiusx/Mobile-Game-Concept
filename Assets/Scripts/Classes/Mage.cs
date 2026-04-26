using UnityEngine;
using Game.Core;

namespace Game.Classes
{
    public class Mage : BaseCharacter
    {
        [SerializeField] private int healthMultiplier = 8;

        public override void InitializeStats()
        {
            CharacterName = "Mage";
            Strength = 3;
            Stamina = 5;
            Intelligence = 12;

            MaxHealth = Stamina * healthMultiplier;
            CurrentHealth = MaxHealth;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            IncreaseStats();
            UpdateHealth();
            UnlockAbilities();
        }

        private void IncreaseStats()
        {
            Strength += 1;
            Stamina += 2;
            Intelligence += 4;
        }

        private void UpdateHealth()
        {
            MaxHealth = Stamina * healthMultiplier;
            RestoreMissingHealthPercent(0.3f);
        }

        private void UnlockAbilities()
        {
            Debug.Log("Mage has unlocked a new ability!");
        }
    }
}
