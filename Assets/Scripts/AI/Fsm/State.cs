using UnityEngine;

namespace Game.AI.Fsm
{
    public abstract class State : ScriptableObject
    {
        public virtual void OnEnter(StateMachine ctx) { }
        public virtual void OnExit(StateMachine ctx) { }
        public virtual void Tick(StateMachine ctx, float dt) { }
    }
}
