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
                var brain = ctx.GetComponent<EnemyBrain>();
                if (brain != null) brain.GoAttack();
                return;
            }

            if (flat.sqrMagnitude <= 0.001f) return;

            ctx.transform.rotation = Quaternion.LookRotation(flat.normalized);
            var cc = ctx.GetComponent<CharacterController>();
            if (cc != null) cc.Move(flat.normalized * moveSpeed * dt);
        }
    }
}
