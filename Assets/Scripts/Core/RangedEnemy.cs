using UnityEngine;

/// <summary>
/// Represents a ranged enemy that attacks from a distance.
/// </summary>
public class RangedEnemy : BaseEnemy
{
    public float attackRange = 10f;
    public float chaseRange = 15f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f; // Configurable projectile speed
    private Transform playerTarget;
    private CharacterController characterController;
    private Rigidbody rb;
    private float attackCooldown = 2f;
    private float lastAttackTime;
    private enum AttackPattern { SingleShot, BurstFire, StaggeredShots }
    private AttackPattern currentAttackPattern = AttackPattern.SingleShot;

    protected override void Start()
    {
        base.Start();
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (characterController == null && rb == null)
        {
            Debug.LogWarning(enemyName + " is missing both CharacterController and Rigidbody. Ensure one is attached for movement.");
        }

        playerTarget = PlayerManager.Instance?.GetPlayerTransform();
    }

    protected override void Update()
    {
        base.Update();
        HandleChase();
    }

    private void HandleChase()
    {
        if (playerTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= chaseRange && distance > attackRange)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            
            if (characterController != null)
            {
                characterController.Move(direction * moveSpeed * Time.deltaTime);
            }
            else if (rb != null)
            {
                rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
            }
        }
    }

    public override void PerformAttack()
    {
        if (playerTarget == null || projectilePrefab == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            ExecuteAttackPattern();
        }
    }

    private void ExecuteAttackPattern()
    {
        switch (currentAttackPattern)
        {
            case AttackPattern.SingleShot:
                ShootProjectile();
                break;
            case AttackPattern.BurstFire:
                StartCoroutine(BurstFireAttack());
                break;
            case AttackPattern.StaggeredShots:
                StartCoroutine(StaggeredShotsAttack());
                break;
        }
    }

    private IEnumerator BurstFireAttack()
    {
        for (int i = 0; i < 3; i++) // Fires 3 shots rapidly
        {
            ShootProjectile();
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator StaggeredShotsAttack()
    {
        for (int i = 0; i < 3; i++) // Fires 3 shots at staggered intervals
        {
            ShootProjectile();
            yield return new WaitForSeconds(1f);
        }
    }

    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.forward, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            rb.velocity = direction * projectileSpeed; // Uses configurable projectile speed
        }
    }
}
