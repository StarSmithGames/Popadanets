using UnityEngine;

namespace Game.Entities.Character
{
    public interface IController
    {
        public bool IsGrounded { get; }
        
        public Vector3 GetVelocity();
        public Vector3 GetMovementVelocity();
    }
}