using Game.Systems.FSMSystem;

namespace Game.Entities.Character
{
    public sealed class GroundedState : State
    {
        public GroundedState(
            StateMachine stateMachine
            ) : base( stateMachine )
        {
        }

        public override void Enter()
        {
            
            
            base.Enter();
        }
    }
}