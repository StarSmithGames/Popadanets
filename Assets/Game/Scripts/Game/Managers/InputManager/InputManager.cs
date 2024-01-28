using Cysharp.Threading.Tasks;
using StarSmithGames.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Managers.InputManager
{
    public sealed class InputManager
    {
        public event Action< KeyCodeBind > OnKeyDown;
        
        private readonly List< KeyCodeBind > _binds;
        private readonly InputSettings _settings;
        
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
                    CheckKeyCode( _binds[i] );
                }
                
                isCanceled = await UniTask.Yield( PlayerLoopTiming.Update, token ).SuppressCancellationThrow();
            }
        }
        
        private void CheckKeyCode( KeyCodeBind bind )
        {
            if ( bind.Codes.Count == 0 ) return;

            if ( Inout.GetKeyDown( bind ) )
            {
                OnKeyDown?.Invoke( bind );
            }
        }
    }
}