using UnityEngine;

/// <summary>
/// Represents a melee-based enemy with close-range attacks.
/// </summary>
public class MeleeEnemy : BaseEnemy
{
    public float attackRange = 1.5f;
    public float chaseRange = 5f;
    private static Transform playerTarget;
    private CharacterController characterController;
    private float attackCooldown = 1.5f;
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogWarning(enemyName + " is missing a CharacterController.");
        }

        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleChase();
    }

    private void HandleChase()
    {
        if (playerTarget == null || characterController == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= chaseRange)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            characterController.Move(direction * moveSpeed * Time.deltaTime);
        }
    }

    public override void PerformAttack()
    {
        if (playerTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log(enemyName + " attacks the player!");
            lastAttackTime = Time.time;
            // Placeholder for attack logic (e.g., dealing damage, animations)
        }
    }
}
