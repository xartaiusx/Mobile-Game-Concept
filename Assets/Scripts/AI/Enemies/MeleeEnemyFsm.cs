using UnityEngine;
using Game.AI.Fsm;

namespace Game.AI.Enemies
{
    [CreateAssetMenu(menuName = "Game/AI/Melee/ChaseState")]
    public class ChaseState : State
    {
        public float moveSpeed = 3f;
        public float attackRange = 1.6f;

        public override void Tick(StateMachine ctx, float dt)
        {
            var enemy = ctx.GetComponent<Game.Core.BaseEnemy>();
            var player = Game.Core.PlayerManager.Instance != null ? Game.Core.PlayerManager.Instance.GetPlayerTransform() : null;
            if (enemy == null || player == null) return;

            Vector3 flat = new Vector3(player.position.x, ctx.transform.position.y, player.position.z) - ctx.transform.position;
            float dist = flat.magnitude;
            if (dist <= attackRange)
            {
                var sm = ctx.GetComponent<EnemyBrain>();
                if (sm != null) sm.GoAttack();
                return;
            }

            ctx.transform.rotation = Quaternion.LookRotation(flat.normalized);
            var cc = ctx.GetComponent<CharacterController>();
            if (cc != null) cc.Move(flat.normalized * moveSpeed * dt);
        }
    }

    [CreateAssetMenu(menuName = "Game/AI/Melee/AttackState")]
    public class AttackState : State
    {
        public float cooldown = 1.0f;

        private float t;

        public override void OnEnter(StateMachine ctx)
        {
            t = 0f;
            // Perform the hit immediately on enter for a snappy feel
            var player = Game.Core.PlayerManager.Instance != null ? Game.Core.PlayerManager.Instance.GetPlayerTransform() : null;
            if (player != null)
            {
                var bc = player.GetComponent<Game.Core.BaseCharacter>();
                if (bc != null) bc.TakeDamage(5);
            }
        }

        public override void Tick(StateMachine ctx, float dt)
        {
            t += dt;
            if (t >= cooldown)
            {
                var sm = ctx.GetComponent<EnemyBrain>();
                if (sm != null) sm.GoChase();
            }
        }
    }

    /// <summary>
    /// Helper MonoBehaviour to hold references to states and drive transitions.
    /// Attach alongside StateMachine on a Melee enemy prefab.
    /// </summary>
    public class EnemyBrain : MonoBehaviour
    {
        [SerializeField] private StateMachine machine;
        [SerializeField] private ChaseState chase;
        [SerializeField] private AttackState attack;

        private void Reset()
        {
            machine = GetComponent<StateMachine>();
        }

        private void Start()
        {
            if (machine != null && chase != null)
                machine.Transition(chase);
        }

        public void GoChase()
        {
            if (machine != null && chase != null) machine.Transition(chase);
        }

        public void GoAttack()
        {
            if (machine != null && attack != null) machine.Transition(attack);
        }
    }
}
