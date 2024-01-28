using UnityEngine;

namespace Game.Managers.InputManager
{
    public static class Inout
    {
        public const float SENSITIVITY = 3f;
        public const float DEADZONE = 0.001f;
        
        public static bool GetKey( KeyCodeBind bind )
        {
            for ( int i = 0; i < bind.Codes.Count; i++ )
            {
                if ( Input.GetKey( bind.Codes[ i ] ) )
                {
                    return true;
                }
            }

            return false;
        }
        
        public static bool GetKeyDown( KeyCodeBind bind )
        {
            for ( int i = 0; i < bind.Codes.Count; i++ )
            {
                if ( Input.GetKeyDown( bind.Codes[ i ] ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}