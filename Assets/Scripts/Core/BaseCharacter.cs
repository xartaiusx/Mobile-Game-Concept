using UnityEngine;
using System;

/// <summary>
/// BaseCharacter defines the core properties and methods for all characters (Player + AI).
/// </summary>
public abstract class BaseCharacter : MonoBehaviour
{
    public string characterName;
    public int level = 1;

    private int strength;
    private int stamina;
    private int intelligence;

    public int Strength
    {
        get => strength;
        set => strength = Mathf.Max(0, value); // Prevents negative values
    }

    public int Stamina
    {
        get => stamina;
        set => stamina = Mathf.Max(0, value);
    }

    public int Intelligence
    {
        get => intelligence;
        set => intelligence = Mathf.Max(0, value);
    }

    public int maxHealth;
    public int currentHealth;

    public event Action<BaseCharacter> OnDeath;

    public abstract void InitializeStats();
    public virtual void LevelUp() {}

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void Die()
    {
        Debug.Log(characterName + " has died.");
        OnDeath?.Invoke(this);
    }
}
