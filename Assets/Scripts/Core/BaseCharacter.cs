using UnityEngine;

/// <summary>
/// BaseCharacter defines the core properties and methods for all characters (Player + AI).
/// </summary>
public class BaseCharacter : MonoBehaviour
{
    public string characterName;
    public int level = 1;

    public int strength;
    public int stamina;
    public int intelligence;

    public int maxHealth;
    public int currentHealth;

    public virtual void InitializeStats() {}
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
    }
}
