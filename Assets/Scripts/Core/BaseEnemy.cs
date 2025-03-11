using UnityEngine;
using System;

/// <summary>
/// Base class for all AI enemies.
/// </summary>
public abstract class BaseEnemy : MonoBehaviour
{
    public string enemyName;
    private int health;
    public int attackDamage;
    public float attackInterval;
    public float moveSpeed;

    protected float attackTimer;
    protected CharacterController controller;
    private bool isInvulnerable;
    private float invulnerabilityDuration = 0.5f;
    private float invulnerabilityTimer;

    public static event Action<BaseEnemy> OnEnemyDefeated;

    public int Health
    {
        get => health;
        private set => health = Mathf.Max(0, value); // Prevents negative health values
    }

    protected virtual void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogWarning(enemyName + " is missing a CharacterController. Ensure this enemy type requires one.");
        }
    }

    protected virtual void Update()
    {
        HandleAttackTimer();
        HandleMovement();
        HandleInvulnerability();
    }

    private void HandleAttackTimer()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            PerformAttack();
            attackTimer = 0;
        }
    }

    private void HandleInvulnerability()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
            }
        }
    }

    protected virtual void HandleMovement()
    {
        // Placeholder for movement logic; override in derived classes
    }

    public abstract void PerformAttack();

    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
        {
            Debug.Log(enemyName + " is invulnerable and did not take damage.");
            return;
        }

        Health -= damage;
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        if (Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(enemyName + " has been defeated.");
        OnEnemyDefeated?.Invoke(this); // Trigger event for additional effects like loot drops
        Destroy(gameObject);
    }
}
