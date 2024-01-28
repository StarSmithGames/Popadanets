using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Game.Managers.InputManager
{
    [ InlineProperty ]
    [ System.Serializable ]
    public sealed class KeyCodeBind
    {
        [ ReadOnly ]
        public string Id;
        public List< UnityEngine.KeyCode > Codes = new();

        public KeyCodeBind(
            string id,
            params UnityEngine.KeyCode[] codes
            )
        {
            Id = id;
            Codes.Clear();
            Codes.AddRange( codes );
        }
    }
}