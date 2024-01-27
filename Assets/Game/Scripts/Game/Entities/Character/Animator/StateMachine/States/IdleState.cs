using Game.Systems.FSMSystem;

namespace Game.Entities.Character
{
    public sealed class IdleState : State
    {
        public IdleState( StateMachine stateMachine ) : base( stateMachine )
        {
        }
    }
}