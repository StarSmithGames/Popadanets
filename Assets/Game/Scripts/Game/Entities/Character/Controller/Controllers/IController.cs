using UnityEngine;

namespace Game.Entities.Character
{
    public interface IController
    {
        public abstract Vector3 GetVelocity();
        public abstract Vector3 GetMovementVelocity();
        public abstract bool IsGrounded();
    }
}