using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// Represents a boss enemy with advanced attack patterns and abilities.
/// </summary>
public class BossEnemy : BaseEnemy
{
    public float attackRange = 12f;
    public float chaseRange = 20f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float specialAttackCooldown = 10f;
    private NavMeshAgent navMeshAgent;
    private float attackCooldown = 2f;
    private float lastAttackTime;
    private float lastSpecialAttackTime;

    private Transform PlayerTarget => PlayerManager.Instance?.GetPlayerTransform();

    private IAttackStrategy attackStrategy;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogWarning(enemyName + " is missing a NavMeshAgent. Ensure this enemy is set up for pathfinding.");
        }

        attackStrategy = new MeleeAttackStrategy(); // Default attack strategy
    }

    protected override void Update()
    {
        base.Update();
        HandleChase();
    }

    private void HandleChase()
    {
        if (PlayerTarget == null || navMeshAgent == null)
            return;

        float distance = Vector3.Distance(transform.position, PlayerTarget.position);
        if (distance <= chaseRange && distance > attackRange)
        {
            navMeshAgent.SetDestination(PlayerTarget.position);
        }
    }

    public override void PerformAttack()
    {
        if (PlayerTarget == null || projectilePrefab == null)
            return;

        float distance = Vector3.Distance(transform.position, PlayerTarget.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            attackStrategy.ExecuteAttack(this, PlayerTarget);
        }

        if (distance <= attackRange && Time.time >= lastSpecialAttackTime + specialAttackCooldown)
        {
            lastSpecialAttackTime = Time.time;
            StartCoroutine(SpecialAttack());
        }
    }

    public void SetAttackStrategy(IAttackStrategy newStrategy)
    {
        attackStrategy = newStrategy;
    }

    private IEnumerator SpecialAttack()
    {
        Debug.Log(enemyName + " performs a powerful special attack!");
        yield return new WaitForSeconds(1f); // Simulate attack charge-up time
        // Placeholder for actual special attack mechanics
    }
}

public interface IAttackStrategy
{
    void ExecuteAttack(BossEnemy enemy, Transform target);
}

public class MeleeAttackStrategy : IAttackStrategy
{
    public void ExecuteAttack(BossEnemy enemy, Transform target)
    {
        Debug.Log(enemy.enemyName + " performs a melee attack!");
        // Placeholder for melee attack logic
    }
}

public class RangedAttackStrategy : IAttackStrategy
{
    public void ExecuteAttack(BossEnemy enemy, Transform target)
    {
        GameObject projectile = Object.Instantiate(enemy.projectilePrefab, enemy.transform.position + enemy.transform.forward, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(target.position, enemy.projectileSpeed);
        }
    }
}
