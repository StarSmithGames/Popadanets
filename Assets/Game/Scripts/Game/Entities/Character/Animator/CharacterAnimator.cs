using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class CharacterAnimator
    {
        private CharacterAnimationsStateMachine _animationsStateMachine;
        private IdleState _idleState;

        private int _isIdleHash;
        private int _isGroundedHash;
        private int _isCrouchedHash;
        private int _forwardHash;
        private int _verticalHash;
        private int _isLandedHash;
        
        private readonly Animator _animator;
        private readonly IController _controller;
        
        public CharacterAnimator(
            Animator animator,
            IController controller
            )
        {
            _animator = animator ?? throw new ArgumentNullException( nameof(animator) );
            _controller = controller ?? throw new ArgumentNullException( nameof(controller) );
            
            _isIdleHash = Animator.StringToHash(AnimatorParams.IS_IDLE);
            _isGroundedHash = Animator.StringToHash(AnimatorParams.IS_GROUNDED);
            _isCrouchedHash = Animator.StringToHash(AnimatorParams.IS_CROUCHED);
            _forwardHash = Animator.StringToHash(AnimatorParams.FORWARD);
            _verticalHash = Animator.StringToHash(AnimatorParams.VERTICAL);
            _isLandedHash = Animator.StringToHash(AnimatorParams.IS_LANDED);

            _animationsStateMachine = new();

            _idleState = new IdleState( _animationsStateMachine );
            _animationsStateMachine.ChangedState( _idleState );

            controller.OnLanded += ControllerLandedHandler;
        }

        public void Tick()
        {
             Vector3 velocity = _controller.GetNormalVelocity();

             bool isIdle = velocity.magnitude == 0;
            
             _animator.SetFloat( _forwardHash, velocity.magnitude );
             _animator.SetFloat( _verticalHash, velocity.y );
            //animator.SetFloat("HorizontalSpeed", Mathf.Clamp(controller.CalculateAngleToDesination(), -90, 90) / 90);

             _animator.SetBool( _isIdleHash, isIdle );
             _animator.SetBool( _isGroundedHash, _controller.IsGrounded );
            
            _animationsStateMachine.Tick();
        }

        private void ControllerLandedHandler( Vector3 _ )
        {
            _animator.SetTrigger( _isLandedHash );
        }
    }
}