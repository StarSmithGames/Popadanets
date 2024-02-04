using UnityEngine.Animations.Rigging;

namespace Game.Entities.Character
{
    [ System.Serializable ]
    public sealed class HumanoidLegsRig
    {
        public Rig Rig;
        public TwoBoneIKConstraint LeftConstraint;
        public TwoBoneIKConstraint RightConstraint;
    }
}