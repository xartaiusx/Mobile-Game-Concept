using UnityEngine;
using Game.AI.Fsm;

namespace Game.AI.Enemies
{
    [CreateAssetMenu(menuName = "Game/AI/Melee/AttackState")]
    public class AttackState : State
    {
        public float cooldown = 1.0f;
        public int damage = 5;

        private float timer;

        public override void OnEnter(StateMachine ctx)
        {
            timer = 0f;
            var player = Game.Core.PlayerManager.Instance != null ? Game.Core.PlayerManager.Instance.GetPlayerTransform() : null;
            if (player == null) return;

            var character = player.GetComponent<Game.Core.BaseCharacter>();
            if (character != null)
                character.TakeDamage(new Game.Core.DamageContext(ctx.gameObject, damage, Game.Core.DamageType.Physical, Game.Rhythm.RhythmGrade.Miss, true));
        }

        public override void Tick(StateMachine ctx, float dt)
        {
            timer += dt;
            if (timer < cooldown) return;

            var brain = ctx.GetComponent<EnemyBrain>();
            if (brain != null)
                brain.GoChase();
        }
    }
}
