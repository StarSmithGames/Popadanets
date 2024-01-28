using System.Collections.Generic;
using UnityEngine;

namespace Game.Managers.InputManager
{
    [ System.Serializable ]
    public sealed class MovementBind
    {
        public AxisBind Horizontal = new(
            InputParams.HORIZONTAL,
            new( "Right", KeyCode.D, KeyCode.RightArrow ),
            new( "Left", KeyCode.A, KeyCode.LeftArrow ));
        public AxisBind Vertical = new(
            InputParams.VERTICAL,
            new( "Up", KeyCode.W, KeyCode.UpArrow ),
            new( "Down", KeyCode.S, KeyCode.DownArrow ));
        public KeyCodeBind Jump = new( InputParams.JUMP, KeyCode.Space);

        public List< KeyCodeBind > GetBindings()
        {
            List< KeyCodeBind > list = new();
            list.AddRange( Horizontal.GetBindings() );
            list.AddRange( Vertical.GetBindings() );
            list.Add( Jump );

            return list;
        }
    }
}