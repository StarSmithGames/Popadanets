using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Entities.Character
{
    public sealed class IKFoot
    {
        private Transform _footPlacement;
        private Quaternion _lastIkRotation;

        private Transform _tip;
        private Transform _target;

        private readonly Settings _settings;
        private readonly Transform _root;
        private readonly TwoBoneIKConstraint _constraint;

        public IKFoot( Settings settings, Transform root, TwoBoneIKConstraint constraint )
        {
            _settings = settings ?? throw new ArgumentNullException( nameof(settings) );
            _root = root ?? throw new ArgumentNullException( nameof(root) );
            _constraint = constraint ?? throw new ArgumentNullException( nameof(constraint) );

            _tip = _constraint.data.tip;
            _target = _constraint.data.target;

            Initialize();
        }

        private void Initialize()
        {
            _footPlacement = new GameObject( "FootPlacement" ).transform;
            _footPlacement.SetParent( _settings.Forward );
            _footPlacement.localPosition = Vector3.zero;
            _footPlacement.rotation = _constraint.data.tip.rotation;
        }

        public void Tick()
        {
            Quaternion boneRotation = _tip.rotation;

            Vector3 origin = _tip.position;
            Ray ray = new( origin, Vector3.down );

            bool isHit = Physics.Raycast( ray, out var hit, _settings.MaxHitDistance );
            if ( isHit )
            {
                var position = _target.position;
                position.y = hit.point.y + _settings.YHitOffset;
                _target.position = position;

                boneRotation = Rotation( hit.normal, _settings.Forward );
                
                DrawHit( _target.position, hit.point, hit.normal );
            }

            //IK
            float dt = 25 * Time.deltaTime;
            _lastIkRotation = Quaternion.Lerp( _lastIkRotation, boneRotation, dt);
            _target.rotation = _lastIkRotation;
        }
        
        //magic
        private Quaternion Rotation( Vector3 normal, Transform footForward )
        {
            Vector3 localNormal = _root.InverseTransformDirection( normal );

            footForward.localRotation = Quaternion.FromToRotation( Vector3.up, localNormal );

            return _footPlacement.rotation;
        }
        
        private void DrawHit( Vector3 targetPosition, Vector3 hitPosition, Vector3 hitNormal )
        {

            Debug.DrawRay( hitPosition, hitNormal, Color.blue, Time.deltaTime );
            var dir = targetPosition - hitPosition;
            Debug.DrawRay( hitPosition, Vector3.Reflect( dir, hitNormal ), Color.magenta, Time.deltaTime );

            Debug.DrawLine( hitPosition, targetPosition );
            float _markerSize = 0.2f;
            Debug.DrawLine( hitPosition + Vector3.up * _markerSize, hitPosition - Vector3.up * _markerSize, Color.green, Time.deltaTime );
            Debug.DrawLine( hitPosition + Vector3.right * _markerSize, hitPosition - Vector3.right * _markerSize, Color.green, Time.deltaTime );
            Debug.DrawLine( hitPosition + Vector3.forward * _markerSize, hitPosition - Vector3.forward * _markerSize, Color.green, Time.deltaTime );
        }

        [ System.Serializable ]
        public sealed class Settings
        {
            public Transform Forward;

            public float MaxHitDistance = 2f;
            public float YHitOffset = 0.15f;
        }
    }
}