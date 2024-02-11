using Game.Systems.CollisionSystem;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Entities.Character
{
    public sealed class IKArmsBehavior : MonoBehaviour
    {
        public TwoBoneIKConstraint IkConstraint;
        public MeshCaster LeftCaster;
        
        public float RangeOut = 3f;
        public float RangeIn = 1.15f;
        public float RangeMin = 0.5f;
        public float YOffset = 3.5f;
        public float CollisionYMin = 1.5f;
        public float HitOffset = 0.3f;
        public Vector3 TargetRotationOffset;
        [ Space ]
        public bool IsDebug = false;

        private Vector3 Origin
        {
            get
            {
                Vector3 origin = transform.position;
                origin.y += YOffset;

                return origin;
            }
        }
        
        private int Layer => Layers.ENVIRONMENT;

        private Transform _target;
        private Transform _elbow;
        private Collider[] _colliders;

        private bool _isHasPoint;
        private Vector3 _currentPoint;

        private void Awake()
        {
            _target = IkConstraint.data.target;
            
            LeftCaster.Mask = Layer;

            IkConstraint.weight = 0f;
            
            LeftCaster.Observer.onCollectionChanged += OnCollisionsChangedHandler;
        }

        private void OnDestroy()
        {
            LeftCaster.Observer.onCollectionChanged -= OnCollisionsChangedHandler;
        }

        private void FixedUpdate()
        {
            UpdateClosestPoint();
            
            if ( _isHasPoint )
            {
                Vector3 origin = Origin;

                Vector3 direction = ( origin - _currentPoint ).normalized;
                float distance = Vector3.Distance( origin, _currentPoint );
                float lerp = Mathf.InverseLerp( RangeOut, RangeIn, distance );
                
                IkConstraint.weight = lerp;
                _target.position = _currentPoint + direction * HitOffset;
                // Vector3 btwPoint = origin + direction * RangeIn;

                Ray ray = new( origin, direction );
                if ( Physics.Raycast( ray, out RaycastHit hit ) )
                {
                    Vector3 localNormal = transform.InverseTransformDirection( hit.normal );

                    _target.localRotation = Quaternion.Euler( Quaternion.FromToRotation( Vector3.up, localNormal ).eulerAngles + TargetRotationOffset );
                    // _target.localRotation = Quaternion.FromToRotation( Vector3.up, localNormal );
                }
            }
            else
            {
                IkConstraint.weight = 0f;
            }
        }
        

        private void UpdateClosestPoint()
        {
            if ( _colliders != null && _colliders.Length > 0 )
            {
                Vector3 origin = Origin;

                Vector3 closestPoint = Vector3.positiveInfinity;
                float minDistance = float.PositiveInfinity;
                for ( int i = 0; i < _colliders.Length; i++ )
                {
                    Vector3 point = _colliders[ i ].ClosestPoint( origin );
                    float distance = Vector3.Distance( origin, point );
                    if ( distance < minDistance )
                    {
                        minDistance = distance;
                        closestPoint = point;
                    }
                }
                
                _isHasPoint = minDistance < float.PositiveInfinity;
                _currentPoint = closestPoint;
            }
            else
            {
                _isHasPoint = false;
            }
        }

        private void OnCollisionsChangedHandler()
        {
            _colliders = LeftCaster.Observer.Observables.ToArray();
        }
        
        private void OnDrawGizmos()
        {
            if ( !IsDebug ) return;
            
            Vector3 origin = Origin;
            Vector3 originOutoffseted = origin + -transform.right * RangeOut;
            Vector3 originInoffseted = origin + -transform.right * RangeIn;
            Vector3 originMinoffseted = origin + -transform.right * RangeMin;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere( originOutoffseted, 0.1f );
            Gizmos.DrawWireSphere( origin, RangeOut );
            Gizmos.DrawLine( originOutoffseted, originOutoffseted + Vector3.down * CollisionYMin );
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere( originInoffseted, 0.1f );
            Gizmos.DrawWireSphere( origin, RangeIn );
            Gizmos.DrawLine( originInoffseted, originInoffseted + Vector3.down * CollisionYMin );
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( originMinoffseted, 0.1f );
            Gizmos.DrawWireSphere( origin, RangeMin );
            Gizmos.DrawLine( originMinoffseted, originMinoffseted + Vector3.down * CollisionYMin );
            
            if( !Application.isPlaying ) return;

            if ( _colliders != null )
            {
                foreach ( var collider in _colliders )
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine( origin, collider.ClosestPoint( origin) );
                }
            }

            if ( _isHasPoint )
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine( origin, _currentPoint );
            }
        }
    }
}