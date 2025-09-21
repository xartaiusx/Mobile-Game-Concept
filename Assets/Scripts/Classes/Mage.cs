public class Mage : BaseCharacter
{
    private int healthMultiplier;

    public Mage(int customHealthMultiplier = 8)
    {
        healthMultiplier = customHealthMultiplier;
    }

    public override void InitializeStats()
    {
        Strength = 3;
        Stamina = 5;
        Intelligence = 12;

        maxHealth = Stamina * healthMultiplier;
        currentHealth = maxHealth;
    }

    public override void LevelUp()
    {
        level++;
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
        maxHealth = Stamina * healthMultiplier;
        
        // Restore a percentage of missing health on level up instead of full reset
        float healthRestorationFactor = 0.3f; // Heals 30% of missing health
        int healthRestored = Mathf.CeilToInt((maxHealth - currentHealth) * healthRestorationFactor);
        currentHealth = Mathf.Min(currentHealth + healthRestored, maxHealth);
    }

    private void UnlockAbilities()
    {
        // Placeholder for adding new abilities or skill trees in future expansions
        Debug.Log("Mage has unlocked a new ability!");
    }
}
