using System;
using UnityEngine;

namespace Game.Managers.InputManager
{
    public sealed class InputService
    {
        public event Action OnKeyJumpDowned;
        
        public Vector3 InputMovement => new Vector3( Input.GetAxis( InputParams.HORIZONTAL ), 0, Input.GetAxis( InputParams.VERTICAL ) );

        private InputManager _inputManager;
        
        public InputService(
            InputManager inputManager
            )
        {
            _inputManager = inputManager ?? throw new ArgumentNullException( nameof(inputManager) );

            _inputManager.OnKeyDown += KeyDownedHandler;
        }

        private void KeyDownedHandler( KeyCode code )
        {
            Debug.LogError( code );
            
            if ( code != KeyCode.Space ) return;
            
            OnKeyJumpDowned?.Invoke();
        }
    }
}