using System;

public class Healer : BaseCharacter
{
    private static readonly float HealthRestorationFactor = GameSettings.HealerHealthRestorationFactor; // Configurable setting from game settings

    public static event Action<Healer> OnLevelUpEffectsApplied;

    public Healer() : base(10) {}

    public override void InitializeStats()
    {
        Strength = 4;
        Stamina = 7;
        Intelligence = 10;

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
        Strength += 1;
        Stamina += 3;
        Intelligence += 3;
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
        Debug.Log("Healer has unlocked a new ability!");
    }

    private void ApplyAdditionalLevelUpEffects()
    {
        // Invoke event-driven approach for extensibility
        OnLevelUpEffectsApplied?.Invoke(this);
        Debug.Log("Healer receives additional level-up bonuses.");
    }
}
