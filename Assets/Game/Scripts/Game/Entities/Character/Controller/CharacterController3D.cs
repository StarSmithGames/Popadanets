using StarSmithGames.Core.Utils;
using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class CharacterController3D
    {
        public bool IsGrounded => _characterController.isGrounded;
        public bool IsCanMove { get; private set; } = true;
        public bool IsFreezed { get; private set; } = false;
        public bool IsWaitAnimation { get; private set; } = false;

        public Vector3 VelocityNormalized => _lastVelocity / _settings.movementSpeed;
        public Vector3 VelocityHorizontal => VectorMath.RemoveDotVector( _lastVelocity, _root.up );
        public Vector3 VelocityVertical => _lastVelocity - VelocityHorizontal;
        
        private Vector3 _lastPosition;
        private Vector3 _lastVelocity;
        private Vector3 _lastGravityVelocity;

        private readonly Settings _settings;
        private readonly Transform _root;
        private readonly CharacterController _characterController;

        public CharacterController3D(
            Settings settings,
            Transform root,
            CharacterController characterController
            )
        {
            _settings = settings ?? throw new ArgumentNullException( nameof(settings) );
            _root = root;
            _characterController = characterController ?? throw new ArgumentNullException( nameof(characterController) );
        }

        public void Tick( Vector3 input )
        {
            Movement( input );
        }

        public void FixedTick()
        {
            ApplyGravity();
        }
        
        private void Movement( Vector3 input )
        {
            _characterController.Move( input * _settings.movementSpeed * Time.deltaTime );
            _lastVelocity = _characterController.velocity;
        }
        
        private void ApplyGravity()
        {
            if ( IsGrounded && _lastGravityVelocity.y < 0 )
            {
                _lastGravityVelocity.y = 0f;
            }
            
            _lastGravityVelocity.y += _settings.gravity * Time.fixedDeltaTime;
            _characterController.Move( _lastGravityVelocity );
        }

        public void Jump()
        {
            if ( !IsGrounded ) return;

            _lastGravityVelocity.y += _settings.jumpHeight;
        }
        
        public float GetVertical()
        {
            Vector3 vertical = VelocityVertical;
            return vertical.magnitude * VectorMath.GetDotProduct( vertical, _root.up );
        } 

        [System.Serializable]
        public class Settings
        {
            public float movementSpeed = 5f;
            public float jumpHeight = 1.0f;
            public float gravity = -9.81f;
        }
    }
}