using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Represents a melee-based enemy with close-range attacks.
    /// </summary>
    public class MeleeEnemy : BaseEnemy
    {
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float chaseRange = 5f;
        private Transform playerTarget;
        private CharacterController characterController;

        protected override void Start()
        {
            base.Start();
            characterController = GetComponent<CharacterController>();
            playerTarget = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerTransform() : null;
        }

        protected override void HandleMovement()
        {
            if (playerTarget == null || characterController == null)
                return;

            Vector3 flat = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z) - transform.position;
            float distance = flat.magnitude;
            if (distance <= chaseRange && distance > attackRange)
            {
                Vector3 direction = flat.normalized;
                transform.rotation = Quaternion.LookRotation(direction);
                characterController.Move(direction * MoveSpeed * Time.deltaTime);
            }
        }

        public override void PerformAttack()
        {
            if (playerTarget == null)
                return;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= attackRange)
            {
                NotifyAttackStarted();
                var character = playerTarget.GetComponent<BaseCharacter>();
                if (character != null)
                    character.TakeDamage(new DamageContext(gameObject, AttackDamage, DamageType.Physical, Game.Rhythm.RhythmGrade.Miss, true));
            }
        }
    }
}
