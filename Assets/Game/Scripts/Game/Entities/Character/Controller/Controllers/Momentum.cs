using CMF;
using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class Momentum
    {
        private Vector3 _momentum = Vector3.zero;

        private Transform _transform;
        
        public Momentum(){}
        
        public Momentum( Transform transform )
        {
            _transform = transform ?? throw new ArgumentNullException( nameof(transform) );
        }

        public static Momentum operator +( Momentum instance, Vector3 vector )
        {
            instance._momentum += vector;
            return instance;
        }

        public static Momentum operator -( Momentum instance, Vector3 vector )
        {
            instance._momentum -= vector;
            return instance;
        }
        
        //Returns 'true' if vertical momentum is above a small threshold;
        public bool IsRisingOrFalling()
        {
            //Calculate current vertical momentum;
            Vector3 _verticalMomentum = VectorMath.ExtractDotVector( _momentum, _transform.up );

            //Setup threshold to check against;
            //For most applications, a value of '0.001f' is recommended;
            float _limit = 0.001f;

            //Return true if vertical momentum is above '_limit';
            return ( _verticalMomentum.magnitude > _limit );
        }
        
        //Add momentum to controller;
        public void AddMomentum( Vector3 value, bool isLocal = false )
        {
            if ( isLocal )
                _momentum = _transform.localToWorldMatrix * _momentum;

            _momentum += value;

            if ( isLocal )
                _momentum = _transform.worldToLocalMatrix * _momentum;
        }
	    
        //Get current momentum;
        public Vector3 GetMomentum( bool isLocal = false )
        {
            Vector3 _worldMomentum = _momentum;
            if ( isLocal )
                _worldMomentum = _transform.localToWorldMatrix * _momentum;

            return _worldMomentum;
        }

        //Set controller momentum directly;
        public void SetMomentum( Vector3 _newMomentum,  bool isLocal = false )
        {
            if ( isLocal )
                _momentum = _transform.worldToLocalMatrix * _newMomentum;
            else
                _momentum = _newMomentum;
        }
    }
}