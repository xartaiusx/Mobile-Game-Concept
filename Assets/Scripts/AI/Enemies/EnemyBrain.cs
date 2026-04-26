using UnityEngine;
using Game.AI.Fsm;

namespace Game.AI.Enemies
{
    /// <summary>
    /// Holds melee FSM state references and drives transitions.
    /// Attach alongside StateMachine on a melee enemy prefab.
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

        private void Awake()
        {
            if (machine == null)
                machine = GetComponent<StateMachine>();
        }

        private void Start()
        {
            GoChase();
        }

        public void GoChase()
        {
            if (machine != null && chase != null)
                machine.Transition(chase);
        }

        public void GoAttack()
        {
            if (machine != null && attack != null)
                machine.Transition(attack);
        }
    }
}
