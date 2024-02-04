using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class BoxColliderSensor : ColliderSensor
    {
        public override Collider Collider => BoxCollider;
        
        public BoxCollider BoxCollider;
        
        protected override void RecalculateCollider()
        {
            Vector3 _size = Vector3.zero;
            _size.x = colliderThickness;
            _size.z = colliderThickness;

            BoxCollider.center = colliderOffset * colliderHeight;

            _size.y = colliderHeight * (1f - stepHeightRatio);
            BoxCollider.size = _size;

            BoxCollider.center = BoxCollider.center + new Vector3(0f, stepHeightRatio * colliderHeight/2f, 0f);
        }
        
        protected override float GetColliderRadius( float safetyDistanceFactor )
        {
            float radius = colliderThickness / 2f * _sensorRadiusModifier;
            return Mathf.Clamp( radius, safetyDistanceFactor, ( BoxCollider.size.y / 2f ) * ( 1f - safetyDistanceFactor ) );
        }
    }
}