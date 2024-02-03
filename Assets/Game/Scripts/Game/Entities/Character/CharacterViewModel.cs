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
        private CharacterController3D _characterController;

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

            _characterController = new( _view.ControllerSettings, _view.transform, _view.Controller );
            _characterAnimator = new( _view.Animator, _characterController );
            
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
                _characterController.Tick( _inputService.InputMovement );
                _characterAnimator.Tick();
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.Update, _tickCancellation.Token ).SuppressCancellationThrow();
            }
        }

        private async UniTask FixedTick()
        {
            bool isCanceled = false;
            while ( !isCanceled )
            {
                _characterController.FixedTick();
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.FixedUpdate, _tickCancellation.Token ).SuppressCancellationThrow();
            }
        }

        private void KeyJumpDownedHandler()
        {
            _characterController.Jump();
        }
    }
}