using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Entities.Character
{
    public sealed class IKLegsBehavior : MonoBehaviour
    {
        public TwoBoneIKConstraint IKLeft;
        public TwoBoneIKConstraint IKRight;
        [ Space ]
        public IKFoot.Settings LegsSettings;

        private IKFoot _left;
        private IKFoot _right;

        private void Awake()
        {
            _left = new( LegsSettings, transform, IKLeft );
            _right = new( LegsSettings, transform, IKRight );
        }

        private void FixedUpdate()
        {
            _left.Tick();
            _right.Tick();
        }
    }
}