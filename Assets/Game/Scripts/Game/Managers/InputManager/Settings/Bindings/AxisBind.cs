using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Game.Managers.InputManager
{
    [ System.Serializable ]
    public sealed class AxisBind
    {
        [ ReadOnly ]
        public string Id;
        public KeyCodeBind Positive;
        public KeyCodeBind Negative;

        public AxisBind(
            string id,
            KeyCodeBind positive,
            KeyCodeBind negative
            )
        {
            Id = id;
            Positive = positive;
            Negative = negative;
        }

        public List< KeyCodeBind > GetBindings()
        {
            return new()
            {
                Positive,
                Negative
            };
        } 
    }
}