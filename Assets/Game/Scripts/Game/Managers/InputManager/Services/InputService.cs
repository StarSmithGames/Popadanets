using System;
using UnityEngine;

namespace Game.Managers.InputManager
{
    public sealed class InputService
    {
        public event Action OnKeyJumpDowned;
        
        public Vector3 InputMovement => new( HorizontalMovement.GetAxis(), 0, VerticalMovement.GetAxis() );
        
        public readonly InputAxis HorizontalMovement;
        public readonly InputAxis VerticalMovement;

        private InputSettings _settings;
        private InputManager _inputManager;
        
        public InputService(
            InputSettings settings,
            InputManager inputManager
            )
        {
            _settings = settings ?? throw new ArgumentNullException( nameof(settings) );
            _inputManager = inputManager ?? throw new ArgumentNullException( nameof(inputManager) );

            HorizontalMovement = new( _settings.MovementBind.Horizontal, false );
            VerticalMovement = new( _settings.MovementBind.Vertical, false );
            
            _inputManager.OnKeyDown += KeyDownedHandler;
        }

        private void KeyDownedHandler( KeyCodeBind bind )
        {
            if ( bind != _settings.MovementBind.Jump ) return;
            
            OnKeyJumpDowned?.Invoke();
        }
    }
}