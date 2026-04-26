using Game.Core;

namespace Game.Classes
{
    public class Fighter : BaseCharacter
    {
        private const int HealthMultiplier = 10;

        public override void InitializeStats()
        {
            CharacterName = "Fighter";
            Strength = 10;
            Stamina = 8;
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
