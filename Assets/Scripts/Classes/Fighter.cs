using Game.Core;

namespace Game.Classes
{
    public class Fighter : BaseCharacter
    {
        private const int HealthMultiplier = 13;

        public override void InitializeStats()
        {
            CharacterName = "Warrior";
            Strength = 11;
            Stamina = 9;
            Intelligence = 3;
            MaxHealth = Stamina * HealthMultiplier;
            CurrentHealth = MaxHealth;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            IncreaseStats();
            UpdateHealth();
        }

        private void IncreaseStats()
        {
            Strength += 3;
            Stamina += 2;
            Intelligence += 1;
        }

        private void UpdateHealth()
        {
            int previousMax = MaxHealth;
            MaxHealth = Stamina * HealthMultiplier;
            CurrentHealth += MaxHealth - previousMax;
        }
    }
}
