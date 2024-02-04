using UnityEngine;

namespace Game.Entities.Character
{
    [RequireComponent( typeof( CapsuleCollider ) )]
    public sealed class CapsuleColliderSensor : ColliderSensor
    {
        public override Collider Collider => CapsuleCollider;
        
        public CapsuleCollider CapsuleCollider;
        
        protected override void RecalculateCollider()
        {
            CapsuleCollider.height = colliderHeight;
            CapsuleCollider.center = colliderOffset * colliderHeight;
            CapsuleCollider.radius = colliderThickness/2f;

            CapsuleCollider.center += new Vector3(0f, stepHeightRatio * CapsuleCollider.height/2f, 0f);
            CapsuleCollider.height *= (1f - stepHeightRatio);

            if(CapsuleCollider.height/2f < CapsuleCollider.radius)
                CapsuleCollider.radius = CapsuleCollider.height/2f;
        }

        protected override float GetColliderRadius( float safetyDistanceFactor )
        {
            float radius = colliderThickness / 2f * _sensorRadiusModifier;
            return Mathf.Clamp( radius, safetyDistanceFactor, ( CapsuleCollider.height / 2f ) * ( 1f - safetyDistanceFactor ) );
        }
    }
}