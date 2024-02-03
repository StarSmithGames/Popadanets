using UnityEngine;

namespace Game.Entities.Character.Sensors
{
    public sealed class SphereColliderSensor : ColliderSensor
    {
        public override Collider Collider => SphereCollider;
        
        public SphereCollider SphereCollider;
        
        protected override void RecalculateCollider()
        {
            SphereCollider.radius = colliderHeight/2f;
            SphereCollider.center = colliderOffset * colliderHeight;

            SphereCollider.center = SphereCollider.center + new Vector3(0f, stepHeightRatio * SphereCollider.radius, 0f);
            SphereCollider.radius *= (1f - stepHeightRatio);
        }
        
        protected override float GetColliderRadius( float safetyDistanceFactor )
        {
            float radius = colliderThickness / 2f * _sensorRadiusModifier;
            return Mathf.Clamp( radius, safetyDistanceFactor, ( SphereCollider.radius / 2f ) * ( 1f - safetyDistanceFactor ) );
        }
    }
}