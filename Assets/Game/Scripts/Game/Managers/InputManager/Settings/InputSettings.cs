using System.Collections.Generic;

namespace Game.Managers.InputManager
{
    [ System.Serializable ]
    public sealed class InputSettings
    {
        public MovementBind MovementBind;

        public List< KeyCodeBind > GetKeys()
        {
            var list = new List< KeyCodeBind >();
            list.AddRange( MovementBind.GetBindings() );
            
            return list;
        }
    }
}