using Cysharp.Threading.Tasks;
using StarSmithGames.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Game.Managers.InputManager
{
    public sealed class InputManager
    {
        public event Action< KeyCode > OnKeyDown;
        public event Action< KeyCode > OnKeyUp;

        private List< KeyCodeBind > _binds;
        
        private InputSettings _settings;
        
        public InputManager(
            InputSettings settings
            )
        {
            _settings = settings ?? throw new ArgumentNullException( nameof(settings) );

            _binds = _settings.GetKeys();
            
            Tick( ThreadingUtils.QuitToken ).Forget();
        }

        private async UniTask Tick( CancellationToken token )
        {
            bool isCanceled = false;
            while ( !isCanceled )
            {
                for ( int i = 0; i < _binds.Count; i++ )
                {
                    CheckKeyCode( _binds[i].Code );
                }
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.Update, token ).SuppressCancellationThrow();
            }
        }

        private void CheckKeyCode( KeyCode code )
        {
            if ( Input.GetKeyDown( code ) )
            {
                OnKeyDown?.Invoke( code );
            }
            
            if( Input.GetKeyUp( code ) )
            {
                OnKeyUp?.Invoke( code );
            }
        }
    }
}