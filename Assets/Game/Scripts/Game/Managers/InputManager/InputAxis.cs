using UnityEngine;

namespace Game.Managers.InputManager
{
    public sealed class InputAxis
    {
        private float _axisSmooth = 0;
        
        public readonly AxisBind Axis;
        public readonly bool IsSmooth;

        public InputAxis(
            AxisBind axis,
            bool isSmooth
            )
        {
            Axis = axis;
            IsSmooth = isSmooth;
        }

        public float GetAxis()
        {
            float axisTarget = 0;
            
            if ( Inout.GetKey( Axis.Negative ) )
            {
                axisTarget = -1f;
            }

            if ( Inout.GetKey( Axis.Positive ) )
            {
                axisTarget = 1f;
            }
            
            return IsSmooth ? SmoothInput( axisTarget ) : axisTarget;
        }

        private float SmoothInput( float target )
        {
            _axisSmooth = Mathf.MoveTowards( _axisSmooth, target, Inout.SENSITIVITY * Time.deltaTime );

            return ( Mathf.Abs( _axisSmooth ) < Inout.DEADZONE ) ? 0f : _axisSmooth;
        }
    }
}