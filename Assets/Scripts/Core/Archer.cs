public class Archer : BaseCharacter
{
    private readonly int healthMultiplier;
    private static readonly float HealthRestorationFactor = 0.3f; // Configurable setting for balance

    public Archer(int customHealthMultiplier = 9)
    {
        healthMultiplier = customHealthMultiplier;
    }

    public override void InitializeStats()
    {
        Strength = 7;
        Stamina = 6;
        Intelligence = 5;

        maxHealth = Stamina * healthMultiplier;
        currentHealth = maxHealth;
    }

    public override void LevelUp()
    {
        level++;
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
        maxHealth = Stamina * healthMultiplier;
        
        // Restore a percentage of missing health on level up instead of full reset
        int healthRestored = Mathf.CeilToInt((maxHealth - currentHealth) * HealthRestorationFactor);
        currentHealth = Mathf.Min(currentHealth + healthRestored, maxHealth);
    }

    private void UnlockAbilities()
    {
        // Placeholder for adding new abilities or skill trees in future expansions
        Debug.Log("Archer has unlocked a new ability!");
    }

    private void ApplyAdditionalLevelUpEffects()
    {
        // Placeholder for additional level-up mechanics such as critical hit boosts or passive skills
        Debug.Log("Archer receives additional level-up bonuses.");
    }
}
