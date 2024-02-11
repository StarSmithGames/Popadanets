using Cysharp.Threading.Tasks;
using Game.Managers.InputManager;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class CharacterViewModel
    {
        private CharacterController3D _characterController;
        private TurnController _turnController;
        private CharacterAnimator _characterAnimator;

        private CancellationTokenSource _tickCancellation;

        private CharacterView _view;
        private InputService _inputService;

        public CharacterViewModel( InputService inputService )
        {
            _inputService = inputService ?? throw new ArgumentNullException( nameof(inputService) );
        }

        public void Initialize( CharacterView view )
        {
            _view = view ?? throw new ArgumentNullException( nameof(view) );

            _characterController = new( _view.ControllerSettings, _view.Avatar.Root );
            _turnController = new( _view.TurnSettings, _characterController, _view.Avatar.Root, _view.Avatar.RootModel );
            _characterAnimator = new( _view.AnimatorSettings, _view.Avatar, _characterController );

            _tickCancellation = new();
            Tick().Forget();
            LateTick().Forget();
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
                _characterAnimator.Tick();
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.Update, _tickCancellation.Token ).SuppressCancellationThrow();
            }
        }

        private async UniTask LateTick()
        {
            bool isCanceled = false;
            while ( !isCanceled )
            {
                _turnController.LateTick();

                isCanceled = await UniTask.Yield( PlayerLoopTiming.PreLateUpdate, _tickCancellation.Token ).SuppressCancellationThrow();
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
        private Vector3 CalculateMovementDirection()
        {
            Vector3 velocity = Vector3.zero;
            Vector3 input = _inputService.InputMovement;

            if ( _view.Avatar.CameraModel == null )
            {
                velocity += _view.transform.right * input.x;
                velocity += _view.transform.forward * input.z;
            }
            else
            {
                velocity += Vector3.ProjectOnPlane( _view.Avatar.CameraModel.right, _view.Avatar.Root.up ).normalized * input.x;
                velocity += Vector3.ProjectOnPlane( _view.Avatar.CameraModel.forward, _view.Avatar.Root.up ).normalized * input.z;
            }

            if ( velocity.magnitude > 1f )
                velocity.Normalize();

            return velocity;
        }
    }
}