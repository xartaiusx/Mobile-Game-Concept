using UnityEngine;
using System.Collections;

namespace Game.Core
{
    /// <summary>
    /// Represents a ranged enemy that attacks from a distance.
    /// </summary>
    public class RangedEnemy : BaseEnemy
    {
        private enum AttackPattern { SingleShot, BurstFire, StaggeredShots }

        [SerializeField] private float attackRange = 10f;
        [SerializeField] private float chaseRange = 15f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private ObjectPool projectilePool;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private AttackPattern currentAttackPattern = AttackPattern.SingleShot;

        private Transform playerTarget;
        private CharacterController characterController;
        private Rigidbody body;

        protected override void Start()
        {
            base.Start();
            characterController = GetComponent<CharacterController>();
            body = GetComponent<Rigidbody>();
            playerTarget = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerTransform() : null;
        }

        protected override void HandleMovement()
        {
            if (playerTarget == null)
                return;

            Vector3 flat = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z) - transform.position;
            float distance = flat.magnitude;
            if (distance <= chaseRange && distance > attackRange)
            {
                Vector3 direction = flat.normalized;
                transform.rotation = Quaternion.LookRotation(direction);

                if (characterController != null)
                    characterController.Move(direction * MoveSpeed * Time.deltaTime);
                else if (body != null)
                    body.MovePosition(transform.position + direction * MoveSpeed * Time.deltaTime);
            }
        }

        public override void PerformAttack()
        {
            if (playerTarget == null || (projectilePrefab == null && projectilePool == null))
                return;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= attackRange)
                ExecuteAttackPattern();
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
            for (int i = 0; i < 3; i++)
            {
                ShootProjectile();
                yield return new WaitForSeconds(0.2f);
            }
        }

        private IEnumerator StaggeredShotsAttack()
        {
            for (int i = 0; i < 3; i++)
            {
                ShootProjectile();
                yield return new WaitForSeconds(1f);
            }
        }

        private void ShootProjectile()
        {
            Vector3 spawnPosition = transform.position + transform.forward;
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            Quaternion rotation = direction.sqrMagnitude > 0f ? Quaternion.LookRotation(direction) : transform.rotation;
            GameObject projectile = projectilePool != null
                ? projectilePool.Spawn(spawnPosition, rotation)
                : Instantiate(projectilePrefab, spawnPosition, rotation);

            if (projectile == null) return;

            var poolable = projectile.GetComponent<PoolableProjectile>();
            if (poolable != null)
            {
                poolable.Initialize(playerTarget.position, projectileSpeed, AttackDamage, projectilePool);
                return;
            }

            var projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
                projectileScript.Initialize(playerTarget.position, projectileSpeed, AttackDamage);
        }
    }
}
