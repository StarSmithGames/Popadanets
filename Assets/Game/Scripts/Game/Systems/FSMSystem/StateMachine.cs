namespace Game.Systems.FSMSystem
{
    public abstract class StateMachine
    {
        private State _currentState;
        
        public void Tick()
        {
            _currentState?.Tick();
        }

        public void FixedTick()
        {
            _currentState?.FixedTick();
        }
        
        public void ChangedState( State state )
        {
            if( _currentState == state) return;
            
            _currentState?.Exit();
            _currentState = state;
            _currentState?.Enter();
        }
    }
}