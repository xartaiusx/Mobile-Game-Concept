public class Fighter : BaseCharacter
{
    private const int HealthMultiplier = 10;

    public override void InitializeStats()
    {
        strength = 10;
        stamina = 8;
        intelligence = 3;

        maxHealth = stamina * HealthMultiplier;
        currentHealth = maxHealth;
    }

    public override void LevelUp()
    {
        level++;
        IncreaseStats();
        UpdateHealth();
    }

    private void IncreaseStats()
    {
        strength += 3;
        stamina += 2;
        intelligence += 1;
    }

    private void UpdateHealth()
    {
        maxHealth = stamina * HealthMultiplier;
        
        // Only reset current health if full healing is desired
        if (currentHealth == maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
