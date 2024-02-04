using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public interface IController
    {
        event Action< Vector3 > OnLanded;
        
        public bool IsGrounded { get; }
        
        public Vector3 GetVelocity();
        public Vector3 GetNormalVelocity();
        public Vector3 GetMovementVelocity();
    }
}