using System;
using UnityEngine;

namespace Game.Systems.FSMSystem
{
    public abstract class State
    {
        public event Action OnEntered;
        public event Action OnExited;
        
        protected StateMachine _stateMachine;
        
        public State(
            StateMachine stateMachine
            )
        {
            _stateMachine = stateMachine;
        }
        
        public virtual void Enter()
        {
            OnEntered?.Invoke();
        }
        public virtual void Tick(){}
        public virtual void FixedTick(){}
        public virtual void Exit()
        {
            OnExited?.Invoke();
        }
    }
}