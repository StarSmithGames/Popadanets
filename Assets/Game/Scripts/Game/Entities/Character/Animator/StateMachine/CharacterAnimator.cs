using Game.Entities.Character.Controller;
using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class CharacterAnimator
    {
        private CharacterAnimationsStateMachine _animationsStateMachine;
        private IdleState _idleState;

        private int _isIdleHash;
        private int _forwardHash;
        private int _verticalHash;
        private int _isGroundedHash;

        private readonly Animator _animator;
        private readonly CharacterController3D _controller;
        
        public CharacterAnimator(
            Animator animator,
            CharacterController3D controller
            )
        {
            _animator = animator ?? throw new ArgumentNullException( nameof(animator) );
            _controller = controller ?? throw new ArgumentNullException( nameof(controller) );
            
            _isIdleHash = Animator.StringToHash("IsIdle");
            _forwardHash = Animator.StringToHash("Forward");
            _verticalHash = Animator.StringToHash("Vertical");
            _isGroundedHash = Animator.StringToHash("IsGrounded");
            
            _animationsStateMachine = new();

            _idleState = new IdleState( _animationsStateMachine );
            _animationsStateMachine.ChangedState( _idleState );
        }

        public void Tick()
        {
            Vector3 velocity = _controller.VelocityNormalized;

            bool isIdle = velocity.magnitude == 0;

            _animator.SetFloat( _forwardHash, velocity.magnitude );
            _animator.SetFloat( _verticalHash, _controller.GetVertical() );
            //animator.SetFloat("HorizontalSpeed", Mathf.Clamp(controller.CalculateAngleToDesination(), -90, 90) / 90);

            _animator.SetBool( _isIdleHash, isIdle );
            _animator.SetBool( _isGroundedHash, _controller.IsGrounded );
            
            _animationsStateMachine.Tick();
        }
    }
}