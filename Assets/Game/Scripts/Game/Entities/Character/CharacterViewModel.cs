using Cysharp.Threading.Tasks;
using Game.Managers.InputManager;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class CharacterViewModel
    {
        private CharacterAnimator _characterAnimator;
        private CharacterWalkerController _characterController;

        private CancellationTokenSource _tickCancellation;

        private CharacterView _view;
        private InputService _inputService;
        
        public CharacterViewModel( 
            InputService inputService
            )
        {
            _inputService = inputService ?? throw new ArgumentNullException( nameof(inputService) );
        }

        public void Initialize( CharacterView view )
        {
            _view = view ?? throw new ArgumentNullException( nameof(view) );

            _characterController = new( _view.ControllerSettings, _view.transform );
            // _characterAnimator = new( _view.Animator, _characterController );
            
            _tickCancellation = new();
            Tick().Forget();
            FixedTick().Forget();

            _inputService.OnKeyJumpDowned += KeyJumpDownedHandler;
        }
        
        ~CharacterViewModel()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            _inputService.OnKeyJumpDowned -= KeyJumpDownedHandler;
            
            _tickCancellation?.Cancel();
            _tickCancellation?.Dispose();
            _tickCancellation = null;
        }
        
        private async UniTask Tick()
        {
            bool isCanceled = false;
            while ( !isCanceled )
            {
                // _characterAnimator.Tick();
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.Update, _tickCancellation.Token ).SuppressCancellationThrow();
            }
        }

        private async UniTask FixedTick()
        {
            bool isCanceled = false;
            while ( !isCanceled )
            {
                _characterController.FixedTick( CalculateMovementDirection() );
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.FixedUpdate, _tickCancellation.Token ).SuppressCancellationThrow();
            }
        }

        private void KeyJumpDownedHandler()
        {
            // _characterController.Jump();
        }
        
        //Calculate and return movement direction based on player input;
        //This function can be overridden by inheriting scripts to implement different player controls;
        private Vector3 CalculateMovementDirection()
        {
            Vector3 velocity = Vector3.zero;
            Vector3 input = _inputService.InputMovement;
            
            //If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
            if(_view.CameraTransform == null)
            {
                velocity += _view.transform.right * input.x;
                velocity += _view.transform.forward * input.z;
            }
            else
            {
                //If a camera transform has been assigned, use the assigned transform's axes for movement direction;
                //Project movement direction so movement stays parallel to the ground;
                velocity += Vector3.ProjectOnPlane(_view.CameraTransform.right, _view.transform.up).normalized * input.x;
                velocity += Vector3.ProjectOnPlane(_view.CameraTransform.forward, _view.transform.up).normalized * input.z;
            }

            //If necessary, clamp movement vector to magnitude of 1f;
            if(velocity.magnitude > 1f)
                velocity.Normalize();

            return velocity;
        }
    }
}