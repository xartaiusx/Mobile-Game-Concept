using UnityEngine;

namespace Game.AI.Fsm
{
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private State initial;

        public State Current { get; private set; }

        private void Start()
        {
            if (initial != null) Transition(initial);
        }

        public void Transition(State next)
        {
            if (Current == next || next == null) return;
            if (Current != null) Current.OnExit(this);
            Current = next;
            Current.OnEnter(this);
        }

        private void Update()
        {
            if (Current != null) Current.Tick(this, Time.deltaTime);
        }
    }
}
