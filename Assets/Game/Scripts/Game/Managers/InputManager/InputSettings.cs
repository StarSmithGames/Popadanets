using System.Collections.Generic;
using UnityEngine;

namespace Game.Managers.InputManager
{
    [ System.Serializable ]
    public sealed class InputSettings
    {
        public KeyCodeBind Left;
        public KeyCodeBind Right;
        public KeyCodeBind Forward;
        public KeyCodeBind Backward;
        public KeyCodeBind Jump;

        public List< KeyCodeBind > GetKeys()
        {
            return new()
            {
                Left,
                Right,
                Forward,
                Backward,
                Jump,
            };
        }
    }
}